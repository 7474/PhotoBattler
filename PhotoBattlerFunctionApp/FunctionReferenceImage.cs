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
using System.Net.Http.Headers;

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

        [FunctionName("ImageShare")]
        public static HttpResponseMessage ImageShare(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/share/{name}")]HttpRequestMessage req,
            string name,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            TraceWriter log)
        {
            var blobName = name;
            var info = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == blobName).First();

            var edgeBase = Environment.GetEnvironmentVariable("EDGE_BASE_PATH");
            var ogUrl = $"{edgeBase}/display/{blobName}";
            var ogImage = CommonHelper.PhotoBlobReference(blobName).Uri;
            var ogTitle = info.ModelName;
            var ogDescription = string.Join(", ", info.Result.Predictions.Take(4).Select(x => $"{x.TagName}: {string.Format("{0:0.00}", x.Probability * 100)}%"));

            // XXX 熱いハードコーディング
            var body = $@"<!DOCTYPE html>
<html><head>
<meta charset=utf-8>
<title>{ogTitle}</title>
<meta name=""twitter:card"" content=""summary"" />
<meta name=""twitter:site"" content=""@koudenpa"" />
<meta name=""twitter:creator"" content=""@koudenpa"" />
<meta property=""og:url"" content=""{ogUrl}"" />
<meta property=""og:title"" content=""{ogTitle}"" />
<meta property=""og:description"" content=""{ogDescription}"" />
<meta property=""og:image"" content=""{ogImage}"" />
<meta http-equiv=""refresh"" content=""0; URL = '{ogUrl}'"" />
</head>
<body>
<h1>{ogTitle}</h1>
<img src=""{ogImage}"">
</body>
</html>
";

            var response = new HttpResponseMessage
            {
                Content = new StringContent(body)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
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

        [FunctionName("ImageParameter")]
        public static HttpResponseMessage ImageParameter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/parameter/{name}")]HttpRequestMessage req,
            string name,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            [Table("Tags")] IQueryable<Tag> tags,
            TraceWriter log)
        {
            var blobName = name;
            var info = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == blobName).First();
            var unit = BattleLogic.AnalyzeParameter(info, tags.ToList());
            log.Info(JsonConvert.SerializeObject(unit));

            return req.CreateJsonResponse(HttpStatusCode.OK, unit);
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
    }
}
