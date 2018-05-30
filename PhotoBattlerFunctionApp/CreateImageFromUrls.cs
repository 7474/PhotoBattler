using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using PhotoBattlerFunctionApp.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System.Collections.Generic;
using System.Linq;

namespace PhotoBattlerFunctionApp
{
    public static class CreateImageFromUrls
    {
        /// <summary>
        /// 画像URL指定でのタグ付けのリクエストを処理する。
        /// 
        /// XXX APIインタフェース的には同一のタグ付けなら複数のURLをバルクでリクエストした方がいい
        /// </summary>
        /// <param name="imageCreateRequest"></param>
        /// <param name="log"></param>
        [FunctionName("CreateImageFromUrls")]
        public static void Run([QueueTrigger("create-image-from-urls")]CreateImageFromUrlsRequest imageCreateRequest, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {imageCreateRequest}");

            // https://docs.microsoft.com/ja-jp/azure/cognitive-services/custom-vision-service/csharp-tutorial
            var CV_ProjectId = Environment.GetEnvironmentVariable("CV_ProjectId");
            var CV_TrainingKey = Environment.GetEnvironmentVariable("CV_TrainingKey");
            //var CV_PredictionKey = Environment.GetEnvironmentVariable("CV_PredictionKey");

            var trainingApi = new TrainingApi() { ApiKey = CV_TrainingKey };
            var projectId = Guid.Parse(CV_ProjectId);
            //var project = new ProjectInfo(trainingApi.GetProject(Guid.Parse(CV_ProjectId)));

            var requestTags = imageCreateRequest.Tags.ToList();
            var existTags = trainingApi.GetTags(projectId);
            var nonExistTagNames = requestTags.Except(existTags.Select(x => x.Name));
            var tagIds = existTags.Where(x => requestTags.Contains(x.Name)).Select(x => x.Id).ToList();
            nonExistTagNames.ToList().ForEach(tagName =>
            {
                // FIXME 同時に同じタグ名を指定された複数のキューを処理すると、重複してタグ作成してしまう。
                var tag = trainingApi.CreateTag(projectId, tagName);
                tagIds.Add(tag.Id);
            });

            var images = new List<ImageUrlCreateEntry>()
            {
                new ImageUrlCreateEntry()
                {
                    Url = imageCreateRequest.Url
                }
            };

            trainingApi.CreateImagesFromUrls(Guid.Parse(CV_ProjectId), new ImageUrlCreateBatch(images, tagIds));
        }
    }
}
