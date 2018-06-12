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
using PhotoBattlerFunctionApp.Models;

namespace PhotoBattlerFunctionApp
{
    public static class FunctionAuth
    {
        [FunctionName("AuthPrincipal")]
        public static HttpResponseMessage GetkPrincipal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/principal")]HttpRequestMessage req,
            [Table("Users")]IQueryable<User> users,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return req.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        isAuthenticated = false
                    });
            }
            // ZUMOで認証するとTypeがFederation、NameがNullなのでプロバイダ毎のインジェクションヘッダを見る
            //var user = new
            //{
            //    type = principal.Identity.AuthenticationType,
            //    name = principal.Identity.Name
            //};
            log.Info(JsonConvert.SerializeObject(principal.Identity, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            }));
            log.Info(JsonConvert.SerializeObject(
                req.Headers.Select(x => $"{x.Key}: {string.Join(",", x.Value)}").ToList()));
            User user = User.FindTwitterUser(req, users);

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/twitter/access_token")]HttpRequestMessage req,
            [Table("Users")]IQueryable<User> users,
            [Table("Users")]ICollector<User> outUsers,
            TraceWriter log)
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
            var user = await GetTwitterAccountAsync(oauthToken, oauthTokenSecret);
            if (users.Where(x => x.PartitionKey == "twitter" && x.RowKey == user.RowKey).ToList().Any())
            {
                // とりあえずアップデートは考えない。その辺考えるなら引数にバインドより普通にSDKでI/Oした方が良さそう
            }
            else
            {
                outUsers.Add(user);
            }

            return req.CreateResponse(HttpStatusCode.OK,
                new
                {
                    oauthToken = oauthToken,
                    oauthTokenSecret = oauthTokenSecret
                });
        }

        private static async Task<User> GetTwitterAccountAsync(string accessToken, string accessToeknSecret)
        {
            // https://developer.twitter.com/en/docs/accounts-and-users/manage-account-settings/api-reference/get-account-verify_credentials
            var url = "https://api.twitter.com/1.1/account/verify_credentials.json";
            var authorization = TwitterHelper.BuildGetAuthorizationHeader(url, accessToken, accessToeknSecret, twitterConfig.ConsumerKey, twitterConfig.ConsumerSecret);
            var httpClient = new HttpClient();
            var requestMsg = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };
            requestMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorization);
            var response = await httpClient.SendAsync(requestMsg);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            dynamic json = JsonConvert.DeserializeObject(responseText);

            return new User()
            {
                PartitionKey = "twitter",
                RowKey = json.id_str,
                Type = "twitter",
                Name = json.screen_name,
                ExtraInfo = responseText
            };
        }
    }
}
