using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Models
{
    /// <summary>
    /// PartitionKey = Image source category.
    /// RowKey = Non rule.
    /// {
    /// PartitionKey: "Amazon",
    /// RowKey: "ASIN + URL Hash"
    /// }
    /// </summary>
    public class CreateImageFromUrlsEntity : TableEntity
    {
        public string Url { get; set; }
        public ICollection<string> Tags { get; set; }
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
}
