using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Extensions
{
    static class HttpExtensions
    {
        /// <summary>
        /// PhotoBattlerでの標準的なJSONレスポンスを生成する。
        /// 生成されるJSONは（ローワー）キャメルケースである。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="statusCode"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateJsonResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value)
        {
            return request.CreateResponse(statusCode, value, new JsonMediaTypeFormatter
            {
                UseDataContractJsonSerializer = false,
                SerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            });
        }
    }
}
