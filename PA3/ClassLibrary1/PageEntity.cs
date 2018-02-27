using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class PageEntity : TableEntity
    {

        public PageEntity()
        {

        }
        public PageEntity(string title, string url, DateTime time)
        {
            this.PartitionKey = new HashUrl(url).encoded;
            this.RowKey = "1";
            this.Title = title;
            this.Url = url;
            this.Pubdate = time;
        }
        public string Title { get; set; }

        public string Url { get; set; }

        public DateTime Pubdate { get; set; }



    }
}
