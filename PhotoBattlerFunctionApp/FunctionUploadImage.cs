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
                return new
                {
                    Category = x.Name.StartsWith("FA:G ")
                        ? "Item"
                        : x.Name == "FA:G"
                            ? "Category"
                            : "Attribute",
                    AttributeType = "None",
                    Name = x.Name
                };
            });
            return req.CreateResponse(HttpStatusCode.OK, tags);
        }
        [FunctionName("ImageUpload")]
        public static async Task<HttpResponseMessage> ImageUpload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/upload")]HttpRequestMessage req,
            [Table("CreateImageFromUrls")] IQueryable<CreateImageFromUrlsEntity> imageUrls,
            [Queue("create-image-from-urls")]ICollector<CreateImageFromUrlsRequest> queueItems,
            [Table("CreateImageFromUrls")]ICollector<CreateImageFromUrlsEntity> outImageUrlTable,
            [Table("PredictedInfo")]ICollector<PredictedInfo> outPredictedTable,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // client
            var CV_ProjectId = Environment.GetEnvironmentVariable("CV_ProjectId");
            var CV_TrainingKey = Environment.GetEnvironmentVariable("CV_TrainingKey");
            var CV_PredictionKey = Environment.GetEnvironmentVariable("CV_PredictionKey");
            var trainingApi = new TrainingApi() { ApiKey = CV_TrainingKey };
            var predictionEndpoint = new PredictionEndpoint() { ApiKey = CV_PredictionKey };
            var projectId = Guid.Parse(CV_ProjectId);

            // collect user
            var user = User.FromRequest(req, Thread.CurrentPrincipal);
            var iuser = user as IUser;

            // collect input
            dynamic data = await req.Content.ReadAsAsync<object>();
            //             var base64Data = Regex.Match(data, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            var dataUrlReg = Regex.Match(data.image as string, @"data:image/(?<type>.+?),(?<data>.+)");
            var image = Convert.FromBase64String(dataUrlReg.Groups["data"].Value);
            ICollection<string> tags = data.tags.ToObject<List<string>>();
            var extension = dataUrlReg.Groups["type"].Value;

            // XXX ÇøÇ·ÇÒÇ∆ÇµÇΩåüèÿ
            var existTags = await trainingApi.GetTagsAsync(projectId);
            tags = tags.Intersect(existTags.Select(x => x.Name)).ToList();

            // setup blob
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerName = "photo";
            var blobName = Guid.NewGuid().ToString() + extension;

            var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(blobName);

            var url = blockBlob.Uri.ToString();
            var source = "Upload";
            var key = blockBlob.Name;

            // upload image
            await blockBlob.UploadFromByteArrayAsync(image, 0, image.Length);

            // queue image
            TrainingImageLogic.AddImage(
                imageUrls, queueItems, outImageUrlTable, log,
                source, url, key, tags, user
                );

            // predict image
            // XXX Ç±Ç¡ÇøÇ‡ÉLÉÖÅ[Ç…ÇµÇΩï˚Ç™Ç¢Ç¢ÇÒÇøÇ·Ç§Ç©
            // https://docs.microsoft.com/ja-jp/azure/cognitive-services/custom-vision-service/csharp-tutorial


            var imageUrl = new ImageUrl()
            {
                Url = url
            };
            var predictResult = await predictionEndpoint.PredictImageUrlAsync(projectId, imageUrl);

            var predicted = new PredictedInfo()
            {
                PartitionKey = source,
                RowKey = key,
                Result = predictResult
            };
            outPredictedTable.Add(predicted);

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                path = blockBlob.Name,
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
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerName = "photo";
            var blobName = name;

            var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(blobName);

            var info = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == blobName).First();

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                path = blockBlob.Name,
                url = blockBlob.Uri,
                result = info.Result
            });
        }
    }
}
