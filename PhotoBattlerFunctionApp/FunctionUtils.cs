using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading;
using Newtonsoft.Json;

namespace PhotoBattlerFunctionApp
{
    public static class FunctionUtils
    {
        [FunctionName("CheckPrincipal")]
        public static HttpResponseMessage CheckPrincipal([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "utils/checkprincipal")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var principal = Thread.CurrentPrincipal;
            // ���F�؎���Identity���V���A���C�Y����Ɠ���̃v���p�e�B���Q�Ƃ����ۂɗ�O���������邽�߁A�ŏ������S�ȃv���p�e�B�݂̂��Q�Ƃ���
            var user = new
            {
                type = principal.Identity.AuthenticationType,
                name = principal.Identity.Name
            };

            return principal.Identity.IsAuthenticated
                ? req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(
                    new
                    {
                        message = "Authenticated.",
                        identity = user
                    }))
                : req.CreateResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(
                    new
                    {
                        message = "Need authentication.",
                        identity = user
                    }));
        }
    }
}
