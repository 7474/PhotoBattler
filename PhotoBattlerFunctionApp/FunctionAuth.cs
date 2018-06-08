using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading;
using Newtonsoft.Json;
using TinyOAuth1;
using System;
using PhotoBattlerFunctionApp.Helpers;
using System.Net.Http.Headers;

namespace PhotoBattlerFunctionApp
{
    public static class FunctionAuth
    {
        [FunctionName("AuthPrincipal")]
        public static HttpResponseMessage GetkPrincipal([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/principal")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var principal = Thread.CurrentPrincipal;
            var user = new
            {
                type = principal.Identity.AuthenticationType,
                name = principal.Identity.Name
            };
            // ZUMOで認証するとTypeがFederation、NameがNullなので実態をみる
            log.Info(JsonConvert.SerializeObject(principal.Identity, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            }));
            log.Info(JsonConvert.SerializeObject(
                req.Headers.Select(x => $"{x.Key}: {string.Join(",", x.Value)}").ToList()));

            return req.CreateResponse(HttpStatusCode.OK,
                new
                {
                    isAuthenticated = principal.Identity.IsAuthenticated,
                    identity = user
                });
        }

        private static TinyOAuthConfig twitterConfig = new TinyOAuthConfig
        {
            AccessTokenUrl = "https://api.twitter.com/oauth/access_token",
            AuthorizeTokenUrl = "https://api.twitter.com/oauth/authorize",
            RequestTokenUrl = "https://api.twitter.com/oauth/request_token",
            ConsumerKey = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY"),
            ConsumerSecret = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET")
        };
        [FunctionName("AuthTwitterRequestToken")]
        public static async Task<HttpResponseMessage> PostTwitterRequestTokenAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/twitter/request_token")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            dynamic data = await req.Content.ReadAsAsync<object>();
            string returnUrl = data.returnUrl;
            var authorization = TwitterHelper.BuildRequestAuthorizationHeader(twitterConfig.RequestTokenUrl, returnUrl, twitterConfig.ConsumerKey, twitterConfig.ConsumerSecret);
            var httpClient = new HttpClient();
            var requestMsg = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(twitterConfig.RequestTokenUrl)
            };
            requestMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorization);

            var response = await httpClient.SendAsync(requestMsg);
            var responseText = await response.Content.ReadAsStringAsync();

            log.Info(responseText);
            if (!response.IsSuccessStatusCode)
            {
                return req.CreateResponse(response.StatusCode, responseText);
            }

            string oauthToken = null;
            string oauthTokenSecret = null;

            var keyValPairs = responseText.Split('&');
            for (var i = 0; i < keyValPairs.Length; i++)
            {
                var splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        oauthToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        oauthTokenSecret = splits[1];
                        break;
                }
            }


            return req.CreateResponse(HttpStatusCode.OK,
                new
                {
                    oauthToken = oauthToken,
                    oauthTokenSecret = oauthTokenSecret
                });
        }
        [FunctionName("AuthTwitterAccessToken")]
        public static async Task<HttpResponseMessage> PostTwitterAccessTokenAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/twitter/access_token")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            dynamic data = await req.Content.ReadAsAsync<object>();
            string paramOauthToken = data.oauthToken;
            string paramOauthVerifier = data.oauthVerifier;
            var authorization = TwitterHelper.BuildAccessAuthorizationHeader(twitterConfig.AccessTokenUrl, paramOauthToken, twitterConfig.ConsumerKey, twitterConfig.ConsumerSecret);
            var httpClient = new HttpClient();
            var requestMsg = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(twitterConfig.AccessTokenUrl),
                Content = new StringContent($"oauth_verifier={paramOauthVerifier}")

            };
            requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            requestMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorization);

            var response = await httpClient.SendAsync(requestMsg);
            var responseText = await response.Content.ReadAsStringAsync();

            log.Info(responseText);
            if (!response.IsSuccessStatusCode)
            {
                return req.CreateResponse(response.StatusCode, responseText);
            }

            string oauthToken = null;
            string oauthTokenSecret = null;

            var keyValPairs = responseText.Split('&');
            for (var i = 0; i < keyValPairs.Length; i++)
            {
                var splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        oauthToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        oauthTokenSecret = splits[1];
                        break;
                }
            }


            return req.CreateResponse(HttpStatusCode.OK,
                new
                {
                    oauthToken = oauthToken,
                    oauthTokenSecret = oauthTokenSecret
                });
        }
    }
}
