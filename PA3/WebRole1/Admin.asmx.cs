
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
    // [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {
        public Admin() {
            
        }

        [WebMethod]
        public string StartCrawling()
        {
            CloudQueueMessage cnn = new CloudQueueMessage("http://www.cnn.com/robots.txt");
            CloudQueueMessage checking = new CloudQueueMessage("https://www.cnn.com/sitemaps/sitemap-profile-2018-02.xml");
            StorageManager.LinkQueue().AddMessage(cnn);
            StorageManager.CommandQueue().AddMessage(new CloudQueueMessage("Start Crawling"));
            return "Initated";
        }


        [WebMethod]
        public string StopCrawling()
        {
            StorageManager.CommandQueue().AddMessage(new CloudQueueMessage("Stop Crawling"));
            return "Stop Crawling";
        }

        [WebMethod]
        public string ClearIndex()
        {
            StorageManager.LinkQueue().Clear();
            StorageManager.CommandQueue().Clear();
            StorageManager.HTMLQueue().Clear();
            StorageManager.GetTable().DeleteIfExists();
            StorageManager.ErrorTable().DeleteIfExists();
            return "Clear Index";
        }

        [WebMethod]
        public string GetLinkQueueCount()
        {
            StorageManager.LinkQueue().FetchAttributes();
            return StorageManager.LinkQueue().ApproximateMessageCount.ToString();
        }

        [WebMethod]
        public string GetHTMLQueueCount()
        {
            StorageManager.HTMLQueue().FetchAttributes();
            return StorageManager.HTMLQueue().ApproximateMessageCount.ToString();
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
        public string GetPageTitle(string url)
        {
            if(!url.StartsWith("http"))
            {
                url = "http://" + url;
            }
            string hashedurl = new HashUrl(url).encoded;
            TableOperation retrieveOperation = TableOperation.Retrieve<PageEntity>(hashedurl, "1");
            TableResult retrievedResult = StorageManager.GetTable().Execute(retrieveOperation);
            if (retrievedResult.Result == null)
            {
                return "No Result";
            }
            else
            {
                return new JavaScriptSerializer().Serialize(retrievedResult.Result);
            }
        }
    }
}
