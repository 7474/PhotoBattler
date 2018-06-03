using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using PhotoBattlerFunctionApp.Models;
using System.Collections.Generic;
using AmazonProductAdvtApi;
using System;
using System.Xml;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using PhotoBattlerFunctionApp.Helpers;

namespace PhotoBattlerFunctionApp
{
    /// <summary>
    /// AmazonのAmazon Product Advertising APIで商品の画像を取得しタグ付けのリクエストをエンキューする。
    /// APIはしばしば503を返却するので実行に失敗した際には適宜再試行する。
    /// </summary>
    public static class FunctionQueueImageFromAmazon
    {
        private const string DESTINATION = "ecs.amazonaws.jp";
        private const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";

        [FunctionName("QueueImageFromAmazon")]
        public static async Task<HttpResponseMessage> QueueImageFromAmazon(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "createimage/amazon")]HttpRequestMessage req,
            [Table("CreateImageFromUrls")] IQueryable<CreateImageFromUrlsEntity> imageUrls,
            [Table("Items")] IQueryable<Item> items,
            [Queue("create-image-from-urls")]ICollector<CreateImageFromUrlsRequest> queueItems,
            [Table("CreateImageFromUrls")]ICollector<CreateImageFromUrlsEntity> outImageUrlTable,
            [Table("Items")]ICollector<Item> outItemTable,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            var imageCount = 0;

            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                string asin = data.asin;
                string name = data.name;
                ICollection<string> tags = data.tags.ToObject<List<string>>();
                log.Info($"asin={asin}, name={name}, tags={string.Join(",", tags)}");

                var AWS_ACCESS_KEY_ID = Environment.GetEnvironmentVariable("PAAPI_ACCESS_KEY_ID");
                var AWS_SECRET_KEY = Environment.GetEnvironmentVariable("PAAPI_SECRET_KEY");
                var PAAPI_ASSOCIATE_TAG = Environment.GetEnvironmentVariable("PAAPI_ASSOCIATE_TAG");

                var helper = new SignedRequestHelper(AWS_ACCESS_KEY_ID, AWS_SECRET_KEY, DESTINATION, PAAPI_ASSOCIATE_TAG);

                IDictionary<string, string> r1 = new Dictionary<string, string>();
                r1["Service"] = "AWSECommerceService";
                //r1["Version"] = "2011-08-01";
                r1["Operation"] = "ItemLookup";
                r1["ItemId"] = asin;
                r1["ResponseGroup"] = "Images";

                var requestUrl = helper.Sign(r1);
                log.Verbose($"requestUrl: {requestUrl}");

                var images = Fetch(requestUrl);
                images.ToList().ForEach((url) =>
                {
                    imageCount++;
                    var rowKey = asin + CommonHelper.MD5Hash(url);
                    var source = "Amazon";

                    queueItems.Add(new CreateImageFromUrlsRequest()
                    {
                        Url = url,
                        Tags = tags
                    });
                    // XXX Existチェックしたいだけなのだが
                    if (imageUrls.Where(y => y.PartitionKey == source && y.RowKey == rowKey).ToList().Count() == 0)
                    {
                        outImageUrlTable.Add(new CreateImageFromUrlsEntity()
                        {
                            PartitionKey = source,
                            RowKey = rowKey,
                            Url = url,
                            Tags = tags
                        });
                        log.Info($"{rowKey} entry to CreateImageFromUrls.");
                    }
                    else
                    {
                        log.Info($"{rowKey} is exist.");
                    }
                });
                if (items.Where(y => y.PartitionKey == "Amazon" && y.RowKey == asin).ToList().Count() == 0)
                {
                    outItemTable.Add(new Item()
                    {
                        PartitionKey = "Amazon",
                        RowKey = asin,
                        Name = name,
                        Tags = tags
                    });
                    log.Info($"{asin} entry to Items.");
                }
                else
                {
                    log.Info($"{asin} is exist.");
                }
            }
            catch (Amazon503Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, ex);
            }
            return req.CreateResponse(HttpStatusCode.OK, new
            {
                imageCount = imageCount
            });
        }

        private static IEnumerable<string> Fetch(string url)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                WebResponse response = request.GetResponse();
                if ((response as HttpWebResponse)?.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    throw new Amazon503Exception();
                }
                XmlDocument doc = new XmlDocument();
                doc.Load(response.GetResponseStream());

                // https://www.ipentec.com/document/amazon-product-advertising-api-lookup-item-images
                XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(doc.NameTable);
                xmlNsManager.AddNamespace("ns", NAMESPACE);

                XmlNodeList errorMessageNodes = doc.GetElementsByTagName("Message", NAMESPACE);
                if (errorMessageNodes != null && errorMessageNodes.Count > 0)
                {
                    String message = errorMessageNodes.Item(0).InnerText;
                    throw new ApplicationException("Error: " + message + " (but signature worked)");
                }
                XmlNodeList nodeList = doc.SelectNodes("/ns:ItemLookupResponse/ns:Items/ns:Item/ns:ImageSets/ns:ImageSet/ns:LargeImage/ns:URL", xmlNsManager);
                var results = new List<string>();
                for (int i = 0; i < nodeList.Count; i++)
                {
                    results.Add(nodeList[i].InnerText);
                }
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        class Amazon503Exception : ApplicationException
        {
        }
    }
}
