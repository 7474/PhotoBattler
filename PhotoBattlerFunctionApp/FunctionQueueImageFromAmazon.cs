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

namespace PhotoBattlerFunctionApp
{
    /// <summary>
    /// Amazon��Amazon Product Advertising API�ŏ��i�̉摜���擾���^�O�t���̃��N�G�X�g���G���L���[����B
    /// API�͂��΂���503��ԋp����̂Ŏ��s�Ɏ��s�����ۂɂ͓K�X�Ď��s����B
    /// </summary>
    public static class FunctionQueueImageFromAmazon
    {
        private const string DESTINATION = "ecs.amazonaws.jp";
        private const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";

        [FunctionName("FunctionQueueImageFromAmazon")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "createimage/amazon")]HttpRequestMessage req,
            [Queue("create-image-from-urls")]ICollector<CreateImageFromUrlsRequest> queueItems,
            [Table("CreateImageFromUrls")]ICollector<CreateImageFromUrlsEntity> outTable,
            [Table("Items")]ICollector<Item> outItemTable,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
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
            images.ToList().ForEach((x) =>
            {
                queueItems.Add(new CreateImageFromUrlsRequest()
                {
                    Url = x,
                    Tags = tags
                });
                try
                {
                    outTable.Add(new CreateImageFromUrlsEntity()
                    {
                        PartitionKey = "Amazon",
                        RowKey = asin + MD5Hash(x),
                        Url = x,
                        Tags = tags
                    });
                }
                catch (Exception ex)
                {
                    // Table �ւ̃G���[�i���ɃL�[�d���j�͖�������
                    log.Warning(ex.Message);
                }
            });
            try
            {
                outItemTable.Add(new Item()
                {
                    PartitionKey = "Amazon",
                    RowKey = asin,
                    Name = name,
                    Tags = tags
                });
            }
            catch (Exception ex)
            {
                // Table �ւ̃G���[�i���ɃL�[�d���j�͖�������
                log.Warning(ex.Message);
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static IEnumerable<string> Fetch(string url)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                WebResponse response = request.GetResponse();
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

        private static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }
    }
}