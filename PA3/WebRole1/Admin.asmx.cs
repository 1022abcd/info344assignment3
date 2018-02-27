
using ClassLibrary1;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for Admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {
        public Admin() {
            
        }

        [WebMethod]
        public string StartCrawling()
        {
            CloudQueueMessage cnn = new CloudQueueMessage("http://www.cnn.com/robots.txt");
            CloudQueueMessage bleacher = new CloudQueueMessage("http://www.bleacherreport.com/robots.txt");
            //CloudQueueMessage checking = new CloudQueueMessage("https://www.cnn.com/sitemaps/sitemap-profile-2018-02.xml");
            StorageManager.LinkQueue().AddMessage(cnn);
            StorageManager.LinkQueue().AddMessage(bleacher);
            StorageManager.CommandQueue().AddMessage(new CloudQueueMessage("startcrawling"));
            return "Initated";
        }


        [WebMethod]
        public string StopCrawling()
        {
            StorageManager.CommandQueue().AddMessage(new CloudQueueMessage("stopcrawling"));
            return "Stop Crawling";

        }

        [WebMethod]
        public string ClearIndex()
        {
            StorageManager.CommandQueue().AddMessage(new CloudQueueMessage("clear"));
            return "Cleared";
        }

        [WebMethod]
        public string GetLinkQueueCount()
        {
            CloudQueue queue = StorageManager.LinkQueue();
            queue.FetchAttributes();
            return queue.ApproximateMessageCount.ToString();
        }

        [WebMethod]
        public string GetHTMLQueueCount()
        {
            CloudQueue queue = StorageManager.HTMLQueue();
            queue.FetchAttributes();
            return queue.ApproximateMessageCount.ToString();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetPerformance()
        {
            var mostRecentPerformance = StorageManager.PerformanceCounterTable().CreateQuery<PerformanceCounterEntity>()
                .Where(x => x.PartitionKey == "PerformanceCounter")
                .Take(1);

            List<string> performances = new List<string>();
            foreach (var performance in mostRecentPerformance)
            {
                performances.Add("" + performance.CPUUsage);
                performances.Add("" + performance.RamAvailable);

            }
            return new JavaScriptSerializer().Serialize(performances);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetErrors()
        {
 
            var allErrors = StorageManager.ErrorTable().CreateQuery<ErrorMessage>()
                .Where(x => x.PartitionKey == "ErrorMessage");

            List<string> errorReport = new List<string>();
            foreach (var errorPage in allErrors)
            {
                errorReport.Add(errorPage.urlLink);
                errorReport.Add(errorPage.errorMessage);
            }

            return new JavaScriptSerializer().Serialize(errorReport);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string LastTenTable()
        {
            var table = StorageManager.GetTable().CreateQuery<PageEntity>()
                .Where(s => s.RowKey == "1")
                //.OrderByDescending(r => r.Timestamp)
                .Take(10);

            List<string> lastten = new List<string>();
            foreach (var data in table)
            {
                lastten.Add(data.Url);
            }
            return new JavaScriptSerializer().Serialize(lastten);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetPageTitle(string link)
        {
            if(!link.StartsWith("http"))
            {
                link = "http://" + link;
            }
            TableResult retrievedResult = StorageManager.GetTable().Execute(TableOperation.Retrieve<PageEntity>(new HashUrl(link).encoded, "1"));
            if (retrievedResult.Result == null)
            {
                return "No Result";
            }
            else
            {
                return new JavaScriptSerializer().Serialize(((PageEntity)retrievedResult.Result).Title);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetState()
        {
            var mostRecentState = StorageManager.PerformanceCounterTable().CreateQuery<PerformanceCounterEntity>()
                .Where(x => x.PartitionKey == "PerformanceCounter")
                .Take(1);

            List<string> states = new List<string>();
            foreach (var state in mostRecentState)
            {
                states.Add("" + state.State);
            }
            return new JavaScriptSerializer().Serialize(states);
        }
    }
}
