using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhotoBattlerFunctionApp.Models
{
    public abstract class OwnedEntity : TableEntity
    {
        private IUser _user;
        public IUser User { get { return _user; } set { _user = value; } }
        public string UserType
        {
            get { return _user?.Type; }
            set
            {
                if (_user != null)
                {
                    _user.Type = value;
                }
                else
                {
                    _user = new UserKey()
                    {
                        Type = value
                    };
                }
            }
        }
        public string UserName
        {
            get { return _user?.Name; }
            set
            {
                if (_user != null)
                {
                    _user.Name = value;
                }
                else
                {
                    _user = new UserKey()
                    {
                        Name = value
                    };
                }
            }
        }
    }
    /// <summary>
    /// PartitionKey = Image source category.
    /// RowKey = Non rule.
    /// {
    /// PartitionKey: "Amazon",
    /// RowKey: "ASIN + URL Hash"
    /// }
    /// {
    /// PartitionKey: "Upload",
    /// RowKey: "Blob name"
    /// }
    /// Blob nameはプレフィクスにlog tail patternを適用する。
    /// https://docs.microsoft.com/ja-jp/azure/cosmos-db/table-storage-design-guide#log-tail-pattern
    /// </summary>
    public class CreateImageFromUrlsEntity : OwnedEntity
    {
        public string Url { get; set; }
        public string ModelName { get; set; }
        public ICollection<string> Tags { get; set; }
        public string TagsJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Tags);
            }
            set
            {
                this.Tags = JsonConvert.DeserializeObject<ICollection<string>>(value);
            }
        }
    }
    /// <summary>
    /// PartitionKey = Item source category.
    /// RowKey = ID.
    /// {
    /// PartitionKey: "Amazon",
    /// RowKey: "ASIN"
    /// }
    /// </summary>
    public class Item : TableEntity
    {
        public string Name { get; set; }
        public ICollection<string> Tags { get; set; }
        public string TagsJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Tags);
            }
            set
            {
                this.Tags = JsonConvert.DeserializeObject<ICollection<string>>(value);
            }
        }
    }
    /// <summary>
    /// Keyは CreateImageFromUrlsEntity と同一とする。
    /// </summary>
    public class PredictedInfo : OwnedEntity
    {
        public string ModelName { get; set; }
        public ImagePrediction Result { get; set; }
        public string ResultJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Result);
            }
            set
            {
                this.Result = JsonConvert.DeserializeObject<ImagePrediction>(value);
            }
        }
    }
    //
    public interface IUser
    {
        string Type { get; set; }
        string Name { get; set; }
    }
    public class UserKey : IUser
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }
    public class User : TableEntity, IUser
    {
        public static User FindTwitterUser(HttpRequestMessage req, IQueryable<User> users)
        {
            var twitterToken = req.Headers.GetValues("X-MS-TOKEN-TWITTER-ACCESS-TOKEN").First();
            // Tokenの先頭がIDなので使っているが相当ダーティ
            var twitterId = twitterToken.Substring(0, twitterToken.IndexOf('-'));
            var user = users.Where(x => x.PartitionKey == "twitter" && x.RowKey == twitterId).ToList().First();
            return user;
        }

        public static User FromRequest(IQueryable<User> users, HttpRequestMessage req, IPrincipal principal)
        {
            // https://stackoverflow.com/questions/37582553/how-to-get-client-ip-address-in-azure-functions-c
            // インタフェースを堅くすると実際的に必要な要素にアクセスできなくて面倒くさい。
            string clientIp = "*";
            try
            {
                IEnumerable<string> headerValues;
                if (req.Headers.TryGetValues("X-Forwarded-For", out headerValues) == true)
                {
                    var xForwardedFor = headerValues.FirstOrDefault();
                    clientIp = xForwardedFor.Split(',').GetValue(0).ToString().Trim();
                }
                else
                {
                    if (req.Properties.ContainsKey("MS_HttpContext"))
                    {
                        clientIp = ((HttpContextWrapper)req.Properties["MS_HttpContext"]).Request.UserHostAddress;
                    }
                }
                clientIp = ((HttpContextWrapper)req.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            catch (Exception)
            {
                // Ignore
            }
            var type = "anonymous";
            var name = clientIp;
            if (principal.Identity.IsAuthenticated)
            {
                var user = FindTwitterUser(req, users);
                if (user != null)
                {
                    return user;
                }
            }
            return new User()
            {
                PartitionKey = type,
                RowKey = name,
                Type = type,
                Name = name,
                Ban = false
            };
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public bool Ban { get; set; }
        public string ExtraInfo { get; set; }
    }
}
