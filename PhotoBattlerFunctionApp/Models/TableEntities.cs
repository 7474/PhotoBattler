using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.WindowsAzure.Storage.Table;
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
    /// </summary>
    public class CreateImageFromUrlsEntity : TableEntity
    {
        public string Url { get; set; }
        public ICollection<string> Tags { get; set; }
        public IUser User { get; set; }
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
    }
    /// <summary>
    /// Keyは CreateImageFromUrlsEntity と同一とする。
    /// </summary>
    public class PredictedInfo : TableEntity
    {
        public ImagePrediction Result { get; set; }
    }
    //
    public interface IUser
    {
        string Type { get; }
        string Name { get; }
    }
    public class User : TableEntity, IUser
    {
        public static User FromRequest(HttpRequestMessage req, IPrincipal principal)
        {
            // https://stackoverflow.com/questions/37582553/how-to-get-client-ip-address-in-azure-functions-c
            // インタフェースを堅くすると実際的に必要な要素にアクセスできなくて面倒くさい。
            string clientIP = ((HttpContextWrapper)req.Properties["MS_HttpContext"]).Request.UserHostAddress;
            var type = principal.Identity.IsAuthenticated
                ? principal.Identity.AuthenticationType
                : "anonymous";
            var name = principal.Identity.IsAuthenticated
                ? principal.Identity.Name
                : clientIP;
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
    }
}
