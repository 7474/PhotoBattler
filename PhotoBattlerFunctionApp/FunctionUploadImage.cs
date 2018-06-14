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
using PhotoBattlerFunctionApp.Extensions;
using PhotoBattlerFunctionApp.Helpers;
using PhotoBattlerFunctionApp.Logics;
using PhotoBattlerFunctionApp.Models;

namespace PhotoBattlerFunctionApp
{
    public static class FunctionUploadImage
    {
        [FunctionName("MasterTags")]
        public static async Task<HttpResponseMessage> Tags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tags")]HttpRequestMessage req,
            TraceWriter log)
        {
            var CV_ProjectId = Environment.GetEnvironmentVariable("CV_ProjectId");
            var CV_TrainingKey = Environment.GetEnvironmentVariable("CV_TrainingKey");
            var trainingApi = new TrainingApi() { ApiKey = CV_TrainingKey };
            var projectId = Guid.Parse(CV_ProjectId);

            var existTags = await trainingApi.GetTagsAsync(projectId);

            var tags = existTags.Select(x =>
            {
                var categories = new string[]
                {
                    "FA:G",
                    "HGUC",
                    "MG",
                    "RG"
                };
                return new
                {
                    Category = categories.Contains(x.Name)
                        ? "Category"
                        : categories.Any(y => x.Name.StartsWith(y))
                            ? "Item"
                            : "Attribute",
                    AttributeType = "None",
                    Name = x.Name
                };
            });
            return req.CreateJsonResponse(HttpStatusCode.OK, tags);
        }
        [FunctionName("ImageUpload")]
        public static async Task<HttpResponseMessage> ImageUpload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/upload")]HttpRequestMessage req,
            [Table("Users")]IQueryable<User> users,
            [Table("CreateImageFromUrls")] IQueryable<CreateImageFromUrlsEntity> imageUrls,
            [Queue("create-image-from-urls")]ICollector<CreateImageFromUrlsRequest> queueItems,
            [Table("CreateImageFromUrls")]ICollector<CreateImageFromUrlsEntity> outImageUrlTable,
            [Table("PredictedInfo")]ICollector<PredictedInfo> outPredictedTable,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

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

            // XXX ちゃんとした検証
            var existTags = await trainingApi.GetTagsAsync(projectId);
            tags = tags.Intersect(existTags.Select(x => x.Name)).ToList();

            // setup blob
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerName = "photo";
            // https://docs.microsoft.com/ja-jp/azure/cosmos-db/table-storage-design-guide#log-tail-pattern
            var invertedTicks = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            var blobName = invertedTicks + "-" + Guid.NewGuid().ToString() + "." + extension;

            var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(blobName);

            var url = blockBlob.Uri.ToString();
            var source = "Upload";
            var key = blockBlob.Name;
            log.Info($"pre upload. blobUri={url}");

            // upload image
            await blockBlob.UploadFromByteArrayAsync(image, 0, image.Length);
            log.Info($"after upload.");

            // queue image
            TrainingImageLogic.AddImage(
                imageUrls, queueItems, outImageUrlTable, log,
                source, url, key, tags, user, modelName
                );
            log.Info($"after queue image data.");

            // predict image
            // XXX こっちもキューにした方がいいんちゃうか
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
                url = url,
                result = predictResult
            });
        }
        [FunctionName("ImagePredicted")]
        public static HttpResponseMessage ImagePredicted(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/predicted/{name}")]HttpRequestMessage req,
            string name,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            TraceWriter log)
        {
            var blobName = name;
            var info = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == blobName).First();

            return req.CreateJsonResponse(HttpStatusCode.OK, ImageInfo.FromNameAndResult(blobName, info));
        }


        [FunctionName("ImagePredictedList")]
        public static HttpResponseMessage ImagePredictedList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/predicted")]HttpRequestMessage req,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            TraceWriter log)
        {
            var startName = req.GetQueryNameValuePairs().Where(x => x.Key == "startName").FirstOrDefault().Value;
            if (string.IsNullOrWhiteSpace(startName))
            {
                startName = "0";
            }
            // https://docs.microsoft.com/ja-jp/azure/cosmos-db/table-storage-design-guide#log-tail-pattern
            var listCount = 10;
            var infos = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey.CompareTo(startName) > 0)
                .Take(listCount)
                .ToList();

            return req.CreateJsonResponse(HttpStatusCode.OK, new
            {
                endName = infos.LastOrDefault()?.RowKey,
                list = infos.Select(x => ImageInfo.FromNameAndResult(x.RowKey, x)).ToList()
            });
        }

        public class ImageInfo
        {
            public static ImageInfo FromNameAndResult(string name, PredictedInfo result)
            {
                return new ImageInfo()
                {
                    name = name,
                    url = CommonHelper.PhotoBlobReference(name)?.Uri.ToString(),
                    result = result
                };
            }

            public string name { get; set; }
            public string url { get; set; }
            public PredictedInfo result { get; set; }
        }
    }
}
