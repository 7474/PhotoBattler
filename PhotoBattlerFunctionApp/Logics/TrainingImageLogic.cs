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
        public static void AddImage(
            IQueryable<CreateImageFromUrlsEntity> imageUrls,
            ICollector<CreateImageFromUrlsRequest> queueItems,
            ICollector<CreateImageFromUrlsEntity> outImageUrlTable,
            TraceWriter log,
            string source,
            string url,
            string key,
            ICollection<string> tags,
            IUser user)
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
                    User = user
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
