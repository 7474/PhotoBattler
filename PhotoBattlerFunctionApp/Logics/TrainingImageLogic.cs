using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using PhotoBattlerFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Logics
{
    class TrainingImageLogic
    {
        /// <summary>
        /// Custom Visionへの画像追加依頼キュー及び、その画像へのタグ指定を記録するためのテーブルへの追加を行う。
        /// </summary>
        /// <param name="imageUrls"></param>
        /// <param name="queueItems"></param>
        /// <param name="outImageUrlTable"></param>
        /// <param name="log"></param>
        /// <param name="source"></param>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="tags"></param>
        /// <param name="user"></param>
        /// <param name="modelName"></param>
        public static void AddImage(
            IQueryable<CreateImageFromUrlsEntity> imageUrls,
            ICollector<CreateImageFromUrlsRequest> queueItems,
            ICollector<CreateImageFromUrlsEntity> outImageUrlTable,
            TraceWriter log,
            string source,
            string url,
            string key,
            ICollection<string> tags,
            IUser user, 
            string modelName = "")
        {
            queueItems.Add(new CreateImageFromUrlsRequest()
            {
                Url = url,
                Tags = tags
            });
            var rowKey = key;
            // XXX Existチェックしたいだけなのだが
            if (imageUrls.Where(y => y.PartitionKey == source && y.RowKey == rowKey).ToList().Count() == 0)
            {
                outImageUrlTable.Add(new CreateImageFromUrlsEntity()
                {
                    PartitionKey = source,
                    RowKey = rowKey,
                    Url = url,
                    Tags = tags,
                    User = user,
                    ModelName = modelName
                });
                log.Info($"{rowKey} entry to CreateImageFromUrls.");
            }
            else
            {
                log.Info($"{rowKey} is exist.");
            }
        }
    }
}
