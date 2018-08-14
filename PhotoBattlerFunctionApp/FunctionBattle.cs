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
            var imageX = ImageInfo.FromNameAndResult(nameX, infoX);
            var imageY = ImageInfo.FromNameAndResult(nameY, infoY);
            var invertedTicks = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            var resultId = invertedTicks + "-" + Guid.NewGuid();
            log.Info(JsonConvert.SerializeObject(unitX));
            log.Info(JsonConvert.SerializeObject(unitY));

            var result = BattleLogic.Battle(unitX, unitY);
            log.Info(JsonConvert.SerializeObject(result));

            var resultInfo = new BattleResultInfo()
            {
                PartitionKey = nameX,
                RevFlag = false,
                ThumbnailUrlX = imageX.ThumbnailUrl,
                ThumbnailUrlY = imageY.ThumbnailUrl,
                RowKey = resultId,
                Result = result
            };
            outBattleResultTable.Add(resultInfo);
            // 検索の便宜上、対戦相手側のキーをパーティションとしたレコードを生成する
            // XXX 効果検証（有効ではない可能性がある）
            var resultInfoR = new BattleResultInfo()
            {
                PartitionKey = nameY,
                RevFlag = true,
                ThumbnailUrlX = imageX.ThumbnailUrl,
                ThumbnailUrlY = imageY.ThumbnailUrl,
                RowKey = resultId,
                Result = result
            };
            outBattleResultTable.Add(resultInfoR);

            return req.CreateJsonResponse(HttpStatusCode.OK, new
            {
                ResultId = resultInfo.RowKey
            });
        }

        [FunctionName("BattleResultList")]
        public static HttpResponseMessage BattleResultList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "battles/results")]HttpRequestMessage req,
            [Table("BattleResult")] IQueryable<BattleResultInfo> resultInfo,
            [Table("PredictedInfo")] IQueryable<PredictedInfo> predictedInfo,
            TraceWriter log)
        {
            var nameX = req.GetQueryNameValuePairs().Where(x => x.Key == "nameX").FirstOrDefault().Value;
            var startName = req.GetQueryNameValuePairs().Where(x => x.Key == "startName").FirstOrDefault().Value;
            if (string.IsNullOrWhiteSpace(startName))
            {
                startName = "0";
            }
            // https://docs.microsoft.com/ja-jp/azure/cosmos-db/table-storage-design-guide#log-tail-pattern
            var listCount = 10;
            IQueryable<BattleResultInfo> resultInfoQuery;
            if (string.IsNullOrWhiteSpace(nameX))
            {
                resultInfoQuery = resultInfo.Where(x => x.RowKey.CompareTo(startName) > 0 && x.RevFlag == false);
            }
            else
            {
                resultInfoQuery = resultInfo.Where(x => x.PartitionKey.Equals(nameX) && x.RowKey.CompareTo(startName) > 0);
            }
            var infos = resultInfoQuery
                .Take(listCount)
                .ToList();

            return req.CreateJsonResponse(HttpStatusCode.OK, new
            {
                endName = infos.LastOrDefault()?.RowKey,
                list = infos.ToList()
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
