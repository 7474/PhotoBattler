using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Helpers
{
    class CommonHelper
    {
        public static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }

        public static CloudBlockBlob CreateBlobReference(string storageAccountConnectionString, string containerName, string blobName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(blobName);
            return blockBlob;
        }
        public static CloudBlockBlob PhotoBlobReference(string blobName)
        {
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerName = "photo";

            return CreateBlobReference(storageAccountConnectionString, containerName, blobName);
        }
        public static CloudBlockBlob PhotoThumbnailBlobReference(string blobName)
        {
            return PhotoBlobReference($"thumb/{blobName}");
        }
    }
}
