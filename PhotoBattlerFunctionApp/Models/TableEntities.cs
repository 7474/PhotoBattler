using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Models
{

    public class CreateImageFromUrlsEntity : TableEntity
    {
        public string Url { get; set; }
        public ICollection<string> Tags { get; set; }
    }
    public class Item : TableEntity
    {
        public string Name { get; set; }
        public ICollection<string> Tags { get; set; }
    }
}
