﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Helpers
{
    public static class TwitterHelper
    {
        // https://github.com/johot/TinyOAuth1/blob/master/LICENSE.md
        // https://github.com/johot/TinyOAuth1/blob/master/TinyOAuth1/TinyOAuth.cs
        private static string GetNonce()
        {
            var rand = new Random();
            return rand.Next().ToString("x") + rand.Next().ToString("x");
        }
        private static string GetTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        private static string ConstructRequestUrl(string url)
        {
            var uri = new Uri(url, UriKind.Absolute);
            var normUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);
            if (!(uri.Scheme == "http" && uri.Port == 80 ||
                  uri.Scheme == "https" && uri.Port == 443))
            {
                normUrl += ":" + uri.Port;
            }

            normUrl += uri.AbsolutePath;

            return normUrl;
        }
        private static string GetSignatureBaseString(string method, string url, List<string> requestParameters)
        {
            var sortedList = new List<string>(requestParameters);
            sortedList.Sort();

            var requestParametersSortedString = string.Join("&", sortedList);

            url = ConstructRequestUrl(url);

            return method.ToUpper() + "&" + Uri.EscapeDataString(url) + "&" +
                   Uri.EscapeDataString(requestParametersSortedString);
        }
        private static string GetSignature(string signatureBaseString, string consumerSecret, string tokenSecret = null)
        {
            var hmacsha1 = new HMACSHA1();

            var key = Uri.EscapeDataString(consumerSecret) + "&" + (string.IsNullOrEmpty(tokenSecret)
                          ? ""
                          : Uri.EscapeDataString(tokenSecret));
            hmacsha1.Key = Encoding.ASCII.GetBytes(key);

            var dataBuffer = Encoding.ASCII.GetBytes(signatureBaseString);
            var hashBytes = hmacsha1.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);

            // .NET Core implementation
            // var signingKey = string.Format("{0}&{1}", consumerSecret, !string.IsNullOrEmpty(requestTokenSecret) ? requestTokenSecret : "");
            // IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(signingKey, BinaryStringEncoding.Utf8);
            // MacAlgorithmProvider hmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            // CryptographicKey macKey = hmacSha1Provider.CreateKey(keyMaterial);
            // IBuffer dataToBeSigned = CryptographicBuffer.ConvertStringToBinary(signatureBaseString, BinaryStringEncoding.Utf8);
            // IBuffer signatureBuffer = CryptographicEngine.Sign(macKey, dataToBeSigned);
            // String signature = CryptographicBuffer.EncodeToBase64String(signatureBuffer);
            // return signature;
        }
        // https://developer.twitter.com/en/docs/basics/authentication/api-reference/request_token
        public static string BuildRequestTokenAuthorizationHeader(string requestTokenUrl, string callbackUrl, string consumerKey, string consumerSecret)
        {
            var nonce = GetNonce();
            var timeStamp = GetTimeStamp();

            var requestParameters = new List<string>
            {
                "oauth_nonce=" + nonce,
                "oauth_callback=" +  Uri.EscapeDataString(callbackUrl),
                "oauth_signature_method=HMAC-SHA1",
                "oauth_timestamp=" + timeStamp,
                "oauth_consumer_key=" + consumerKey,
                "oauth_version=1.0"
            };

            var singatureBaseString = GetSignatureBaseString("POST", requestTokenUrl, requestParameters);
            var signature = GetSignature(singatureBaseString, consumerSecret);

            requestParameters.Add("oauth_signature=" + Uri.EscapeDataString(signature));

            return string.Join(", ", requestParameters);
        }
    }
}