using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using PhotoBattlerFunctionApp.Extensions;
using PhotoBattlerFunctionApp.Logics;
using PhotoBattlerFunctionApp.Models;

namespace PhotoBattlerFunctionApp
{
    public static class FunctionBattle
    {
        [FunctionName("BattlePost")]
        public static async Task<HttpResponseMessage> BattlePostAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "battles")]HttpRequestMessage req,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            [Table("Tags")] IQueryable<Tag> tags,
            [Table("BattleResult")]ICollector<BattleResultInfo> outBattleResultTable,
            TraceWriter log)
        {
            dynamic data = await req.Content.ReadAsAsync<object>();
            string nameX = data.nameX;
            string nameY = data.nameY;
            var infoX = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == nameX).First();
            var infoY = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == nameY).First();
            var unitX = BattleLogic.AnalyzeParameter(infoX, tags.ToList());
            var unitY = BattleLogic.AnalyzeParameter(infoY, tags.ToList());
            var resultId = nameX + "-" + Guid.NewGuid().ToString();
            log.Info(JsonConvert.SerializeObject(unitX));
            log.Info(JsonConvert.SerializeObject(unitY));

            var result = BattleLogic.Battle(unitX, unitY);
            log.Info(JsonConvert.SerializeObject(result));

            var resultInfo = new BattleResultInfo()
            {
                PartitionKey = nameY,
                RowKey = resultId,
                Result = result
            };
            outBattleResultTable.Add(resultInfo);

            return req.CreateJsonResponse(HttpStatusCode.OK, new
            {
                ResultKey = resultInfo.PartitionKey,
                ResultId = resultInfo.RowKey
            });
        }

        [FunctionName("BattleResult")]
        public static HttpResponseMessage BattleResult(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "battles/results/{id}")]HttpRequestMessage req,
            string id,
            [Table("BattleResult")] IQueryable<BattleResultInfo> resultInfo,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            TraceWriter log)
        {
            var result = resultInfo.Where(x => x.RowKey == id).First();
            log.Info(JsonConvert.SerializeObject(result));
            var imageX = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == result.Result.UnitX.PredictedInfoKey).First();
            var imageY = predictedInfo.Where(x => x.PartitionKey == "Upload" && x.RowKey == result.Result.UnitY.PredictedInfoKey).First();

            return req.CreateJsonResponse(HttpStatusCode.OK, new
            {
                imageX = ImageInfo.FromNameAndResult(imageX.RowKey, imageX),
                imageY = ImageInfo.FromNameAndResult(imageY.RowKey, imageY),
                result.Result
            });
        }
    }
}
