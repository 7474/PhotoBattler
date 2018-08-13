using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PhotoBattlerFunctionApp.Extensions;
using PhotoBattlerFunctionApp.Helpers;
using PhotoBattlerFunctionApp.Logics;
using PhotoBattlerFunctionApp.Models;
using ImageResizer.ExtensionMethods;
using ImageResizer;
using OpenCvSharp;

namespace PhotoBattlerFunctionApp
{
    public static class FunctionUploadImage
    {
        [FunctionName("ImageUpload")]
        public static async Task<HttpResponseMessage> ImageUpload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/upload")]HttpRequestMessage req,
            [Table("Users")]IQueryable<User> users,
            [Table("CreateImageFromUrls")]IQueryable<CreateImageFromUrlsEntity> imageUrls,
            [Queue("create-image-from-urls")]ICollector<CreateImageFromUrlsRequest> queueItems,
            [Table("CreateImageFromUrls")]ICollector<CreateImageFromUrlsEntity> outImageUrlTable,
            [Table("PredictedInfo")]ICollector<PredictedInfo> outPredictedTable,
            TraceWriter log,
            Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            log.Info("C# HTTP trigger function processed a request.");
            // 実行パスの確認用
            log.Info(Environment.CurrentDirectory);

            // collect input
            dynamic data = await req.Content.ReadAsAsync<object>();
            string imageData = data.image;
            ICollection<string> tags = data.tags.ToObject<List<string>>();
            string modelName = data.modelName;
            log.Info($"modelName={modelName}tags={string.Join(",", tags)}, image={imageData}");

            var dataUrlReg = Regex.Match(imageData, @"data:image/(?<type>.+?);base64,(?<data>.+)");
            var image = Convert.FromBase64String(dataUrlReg.Groups["data"].Value);
            var extension = dataUrlReg.Groups["type"].Value;

            // client
            var CV_ProjectId = Environment.GetEnvironmentVariable("CV_ProjectId");
            var CV_TrainingKey = Environment.GetEnvironmentVariable("CV_TrainingKey");
            var CV_PredictionKey = Environment.GetEnvironmentVariable("CV_PredictionKey");
            var trainingApi = new TrainingApi() { ApiKey = CV_TrainingKey };
            var predictionEndpoint = new PredictionEndpoint() { ApiKey = CV_PredictionKey };
            var projectId = Guid.Parse(CV_ProjectId);

            // collect user
            var user = User.FromRequest(users, req, Thread.CurrentPrincipal);
            var iuser = user as IUser;

            // 既存のタグのみを受け付ける
            var existTags = await trainingApi.GetTagsAsync(projectId);
            tags = tags.Intersect(existTags.Select(x => x.Name)).ToList();

            // setup blob
            // https://docs.microsoft.com/ja-jp/azure/cosmos-db/table-storage-design-guide#log-tail-pattern
            var invertedTicks = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            var blobName = invertedTicks + "-" + Guid.NewGuid().ToString();

            var blockBlob = CommonHelper.PhotoBlobReference(blobName);
            var blockBlobThumbnail = CommonHelper.PhotoThumbnailBlobReference(blobName);
            blockBlob.Properties.ContentType = "image/jpeg";
            blockBlobThumbnail.Properties.ContentType = "image/jpeg";

            var url = blockBlob.Uri.ToString();
            var source = "Upload";
            var key = blockBlob.Name;
            log.Info($"pre upload. blobUri={url}");

            // XXX この辺りの一連の手続きをいい感じに関数分割してフロー処理できると可用性と変更影響範囲が良くなりそう
            // ただ、この程度の規模のサービスで1関数で同期的に処理する平易さ以上のメリットを得られるかは疑問

            // normalize image
            MemoryStream normalizedImage = NormalizeImage(image);
            AnchorLocation anchor = DetectFocusAnchor(log, normalizedImage, context.FunctionAppDirectory);
            log.Info($"Anchor: {anchor.ToString()}");
            MemoryStream thumbnailImage = ToThumbnailImage(image, anchor);

            // upload image
            normalizedImage.Position = 0;
            thumbnailImage.Position = 0;
            await blockBlob.UploadFromStreamAsync(normalizedImage);
            await blockBlobThumbnail.UploadFromStreamAsync(thumbnailImage);
            log.Info($"after upload.");

            // queue image for training
            // 使用しているAPIの都合上、BLOBがアップロード後である必要がある
            TrainingImageLogic.AddImage(
                imageUrls, queueItems, outImageUrlTable, log,
                source, url, key, tags, user, modelName
                );
            log.Info($"after queue image data.");

            // predict image
            // https://docs.microsoft.com/ja-jp/azure/cognitive-services/custom-vision-service/csharp-tutorial
            var imageUrl = new ImageUrl()
            {
                Url = url
            };
            // XXX Storage emurator で通すならNgrock等の工夫が必要。単にDevelop用のStorage Accountを取ってしまった方が楽かも。
            var predictResult = await predictionEndpoint.PredictImageUrlAsync(projectId, imageUrl);
            log.Info($"after prediction.");
            var predicted = new PredictedInfo()
            {
                PartitionKey = source,
                RowKey = key,
                Result = predictResult,
                User = user,
                ModelName = modelName
            };
            outPredictedTable.Add(predicted);

            return req.CreateJsonResponse(HttpStatusCode.OK, new
            {
                name = blockBlob.Name,
                url,
                result = predictResult
            });
        }

        private static MemoryStream NormalizeImage(byte[] image)
        {
            var normalizedImage = new MemoryStream();
            ImageBuilder.Current.Build(new ImageJob(new MemoryStream(image), normalizedImage, new Instructions()
            {
                Width = 1920,
                Height = 1920,
                Mode = FitMode.Max,
                Scale = ScaleMode.DownscaleOnly
            }));
            return normalizedImage;
        }

        private static MemoryStream ToThumbnailImage(byte[] image, AnchorLocation anchor)
        {
            var thumbnailImage = new MemoryStream();
            ImageBuilder.Current.Build(new ImageJob(new MemoryStream(image), thumbnailImage, new Instructions()
            {
                Width = 256,
                Height = 256,
                Mode = FitMode.Crop,
                Scale = ScaleMode.Both,
                Anchor = anchor
            }));
            return thumbnailImage;
        }

        // アニメ顔を認識して顔がある位置のアンカーを返す
        private static AnchorLocation DetectFocusAnchor(TraceWriter log, MemoryStream normalizedImage, string functionAppDirectory)
        {
            var haarCascade = new CascadeClassifier(Path.Combine(functionAppDirectory, "lbpcascade_animeface.xml"));
            normalizedImage.Position = 0;
            using (var faceMat = Mat.FromStream(normalizedImage.CopyToMemoryStream(), ImreadModes.AnyColor))
            {
                var faces = haarCascade.DetectMultiScale(faceMat);
                if (faces.Length == 0)
                {
                    return AnchorLocation.MiddleCenter;
                }
                var face = faces.First();
                log.Info(JsonConvert.SerializeObject(faces));

                var focusX = (face.Left + face.Right) / 2;
                var focusY = (face.Top + face.Bottom) / 2;
                //TopLeft = 1,
                //TopCenter = 2,
                //TopRight = 4,
                //MiddleLeft = 16,
                //MiddleCenter = 32,
                //MiddleRight = 64,
                //BottomLeft = 256,
                //BottomCenter = 512,
                //BottomRight = 1024
                // 概ねどのあたりに焦点があったか？　でアンカーを決める
                int xFactor = 0;
                if (faceMat.Width * 0.33 > focusX)
                {
                    xFactor = 0;
                }
                else if (faceMat.Width * 0.67 < focusX)
                {
                    xFactor = 2;
                }
                else
                {
                    xFactor = 1;
                }
                int yFactor = 0;
                if (faceMat.Height * 0.33 > focusY)
                {
                    yFactor = 1;
                }
                else if (faceMat.Height * 0.67 < focusY)
                {
                    yFactor = 256;
                }
                else
                {
                    yFactor = 16;
                }
                return (AnchorLocation)(yFactor << xFactor);
            }
        }
    }
}
