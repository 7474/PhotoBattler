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

namespace PhotoBattlerFunctionApp
{
    public static class FunctionReferenceImage
    {
        [FunctionName("ImageAsin")]
        public static HttpResponseMessage ImageAsin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/asin/{name}")]HttpRequestMessage req,
            string name,
            [Table("Items")] IQueryable<Item> items,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            TraceWriter log)
        {
            var blobName = name;
            var info = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == blobName).First();

            // XXX 逆インデックスのTableを作成する
            var labels = info.Result.Predictions.ToDictionary(x => x.TagName, x => x.Probability);
            log.Info(JsonConvert.SerializeObject(labels));

            var asinItems = items
                .Where(x => x.PartitionKey == "Amazon").ToList()
                .Where(x => labels.Keys.Contains(x.Name))
                .OrderByDescending(x => labels[x.Name]).Take(4).ToList();
            log.Info(JsonConvert.SerializeObject(asinItems));

            if (asinItems.Count == 0)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            else
            {
                return req.CreateJsonResponse(HttpStatusCode.OK,
                    asinItems.Select(item =>
                        new
                        {
                            asin = item.RowKey,
                            name = item.Name,
                            probability = labels[item.Name]
                        }));
            }
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
                    thumbnailUrl = CommonHelper.PhotoThumbnailBlobReference(name)?.Uri.ToString(),
                    result = result
                };
            }

            public string name { get; set; }
            public string url { get; set; }
            public string thumbnailUrl { get; set; }
            public PredictedInfo result { get; set; }
        }
    }
}
