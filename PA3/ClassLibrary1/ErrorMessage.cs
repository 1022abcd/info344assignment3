using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class ErrorMessage : TableEntity
    {
        public ErrorMessage() {

        }
        public ErrorMessage(string url, string errorMessage)
        {
            this.PartitionKey = "ErrorMessage";
            this.RowKey = new HashUrl(url).encoded;
            this.urlLink = url;
            this.errorMessage = errorMessage;
        }

        public string urlLink { get; set; }
        public string errorMessage { get; set; }
    }
}
