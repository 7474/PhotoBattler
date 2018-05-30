using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Models
{
    public class CreateImageFromUrlsRequest
    {
        public string Url { get; set; }
        public ICollection<string> Tags { get; set; }
    }
}
