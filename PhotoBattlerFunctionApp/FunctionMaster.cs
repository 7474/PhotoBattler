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
    public static class FunctionMaster
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
                // XXX 早くこういうのやめよう
                var categories = new string[]
                {
                    "FA:G",
                    "メガミデバイス",
                    "メガミ",
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
                    x.Name
                };
            });
            return req.CreateJsonResponse(HttpStatusCode.OK, tags);
        }
    }
}
