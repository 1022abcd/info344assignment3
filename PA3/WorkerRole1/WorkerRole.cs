using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClassLibrary1;
using HtmlAgilityPack;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using RobotsTxt;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static string state;
        private static ConcurrentBag<string> disallowedUrl;
        //private static int numberOfUrlsCrawled;
        private static int CpuUsage;
        private static int RamAvailable;
        private static HashSet<string> HtmlSet;
        private static Dictionary<Uri, Robots> robotsdisc;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            disallowedUrl = new ConcurrentBag<string>();
            robotsdisc = new Dictionary<Uri, Robots>();
            state = "Idle";
            //CpuUsage = 0;
            //RamAvailable = 0;
            HtmlSet = new HashSet<string>();

            while(true)
            {

                new Task(GetPerfCounters).Start();
                CloudQueueMessage commandMessage = StorageManager.CommandQueue().GetMessage();
                if (commandMessage != null)
                {
                    switch (commandMessage.AsString)
                    {
                        case "Start Crawling":
                            state = "Loading";
                            break;
                        case "Stop Crawling":
                            state = "Idle";
                            break;
                        default:
                            break;
                    }
                    StorageManager.CommandQueue().DeleteMessage(commandMessage);
                    if (state.Equals("Loading") || state.Equals("Crawling"))
                    {
                            
                            CrawlUrl();
                            GetHTMLData();
                            new Task(GetPerfCounters).Start();
                    }
                }
            }
        }

        private void CrawlUrl()
        {
            while (state.Equals("Loading") && !state.Equals("Idle"))
            {
                if (state.Equals("Idle"))
                {
                    return;
                }
                new Task(GetPerfCounters).Start();
                Thread.Sleep(100);
                CloudQueueMessage linkMessage = StorageManager.LinkQueue().GetMessage();
                if (linkMessage != null)
                {
                    string stringifiedLink = linkMessage.AsString;

                    // if the message is robots.txt
                    if (stringifiedLink.EndsWith("robots.txt"))
                    {
                        HandleRobotstxt(stringifiedLink);
                        stringifiedLink = "";
                        StorageManager.LinkQueue().DeleteMessage(linkMessage);
                    }
                    // if the message is url.xml
                    else
                    {
                        // if the link contains more xml links
                        if (stringifiedLink.Contains("-index"))
                        {
                            CrawlSiteMapIndex(stringifiedLink);
                        }
                        // if the link contains html links
                        else
                        {
                            CrawlSiteMap(stringifiedLink);
                        }
                        StorageManager.LinkQueue().DeleteMessage(linkMessage);
                        if(StorageManager.LinkQueue().GetMessage() == null)
                        {
                            state = "Crawling";
                        }
                    }
                    
                }
            }
        }

        private void HandleRobotstxt(string message)
        {
            using (WebClient client = new WebClient())
            {
                StreamReader reader = new StreamReader(client.OpenRead(message));
                Uri mainUri = new Uri(message.Replace("/robots.txt", ""));
                while (!reader.EndOfStream)
                {
                    if (state.Equals("Idle"))
                    {
                        return;
                    }
                    String line = reader.ReadLine();
                    if (line.Contains("Sitemap"))
                    {
                        if (line.Contains("cnn.com") || line.Contains("/nba"))
                        {
                            line = line.Replace("Sitemap: ", "");
                            StorageManager.LinkQueue().AddMessageAsync(new CloudQueueMessage(line));
                        }
                    }
                    // if the line starts with "Disallow"
                    else if (line.Contains("Disallow"))
                    {
                        line = line.Replace("Disallow: ", "");
                        string disallowedLink = mainUri.OriginalString + line;
                        disallowedUrl.Add(disallowedLink);
                    }
                }
            }
            if (disallowedUrl.Count >= 40)
            {
                state = "Crawling";
            }
        }

        private void CrawlSiteMapIndex(string link)
        {
            string cnn = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement sitemap = XElement.Load(link);
            XName sitemaps = XName.Get("sitemap", cnn);
            XName loc = XName.Get("loc", cnn);
            XName time = XName.Get("lastmod", cnn);
            DateTime givendate = new DateTime(2017, 12, 1);
            DateTime publishDate;

            foreach (var sitemapElement in sitemap.Elements(sitemaps))
            {
                string locElement = sitemapElement.Element(loc).Value;
                publishDate = DateTime.Parse(sitemapElement.Element(time).Value);

                if (publishDate > givendate)
                {
                    StorageManager.LinkQueue().AddMessage(new CloudQueueMessage(locElement));
                }
            }


        }

        private void CrawlSiteMap(string link)
        {
            XElement whole = XElement.Load(link);
            XName url;
            XName loc;
            string selectLink = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XName lastmod = XName.Get("lastmod", "http://www.sitemaps.org/schemas/sitemap/0.9");
            XName news = XName.Get("news", "http://www.google.com/schemas/sitemaps-news/0.9");
            XName newsPublicationDate = XName.Get("publication_date", "http://www.google.com/schemas/sitemaps-news/0.9");
            XName video = XName.Get("video", "http://www.google.com/schemas/sitemap-video/1.1");
            XName videoPublicationDate = XName.Get("publication_date", "http://www.google.com/schemas/sitemap-video/1.1");
            if (link.Contains("bleacher.com"))
            {
                selectLink = "http://www.google.com/schemas/sitemap/0.9";
            }

            url = XName.Get("url", selectLink);
            loc = XName.Get("loc", selectLink);

            DateTime givendate = new DateTime(2017, 12, 1);
            DateTime publishDate = new DateTime(1000, 01, 01);
            try
            {
                foreach (var urlElement in whole.Elements(url))
                {
                    string locElement = urlElement.Element(loc).Value;
                    if (urlElement.Element(news) != null)
                    {
                        publishDate = DateTime.Parse(urlElement.Element(news).Element(newsPublicationDate).Value);
                    }
                    else if (urlElement.Element(video) != null)
                    {
                        publishDate = DateTime.Parse(urlElement.Element(video).Element(videoPublicationDate).Value);
                    } 
                    else if (urlElement.Element(lastmod) != null)
                    {
                        publishDate = DateTime.Parse(urlElement.Element(lastmod).Value);
                    }

                    if(publishDate != null)
                    {
                        if (publishDate > givendate)
                        {
                            if (!HtmlSet.Contains(locElement))
                            {
                                HtmlSet.Add(locElement);
                                StorageManager.HTMLQueue().AddMessage(new CloudQueueMessage(locElement));
                            }
                        }
                    }               
                }
            }
            catch(Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }
            
        }

        private void GetHTMLData()
        {
            HashSet<string> tableList = new HashSet<string>();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument webpage = new HtmlDocument();

            while (state.Equals("Crawling") && !state.Equals("Idle"))
            {
                if (state.Equals("Idle"))
                {
                    return;
                }
                new Task(GetPerfCounters).Start();
                Thread.Sleep(100);
                CloudQueueMessage htmllink = StorageManager.HTMLQueue().GetMessage();
                string url = htmllink.AsString;
                Uri currentUri = new Uri(url);
                if (htmllink == null)
                {
                    break;
                }
                else if (!tableList.Contains(url))
                {
                    tableList.Add(url);
                    using(var client = new WebClient())
                    {
                        try
                        {
                            webpage.LoadHtml(client.DownloadString(url));
                            DateTime pubdlication;
                            
                            HtmlNode pubdateHTML = webpage.DocumentNode.SelectSingleNode("//head/meta[@name='lastmod']");
                            if (pubdateHTML != null)
                            {
                                pubdlication = DateTime.Parse(pubdateHTML.Attributes["content"].Value);
                            }
                            else
                            {
                                pubdlication = DateTime.Today;
                            }
                            string scrapeLink = "http://" + currentUri.Host + currentUri.PathAndQuery;
                            string title = webpage.DocumentNode.SelectSingleNode("//head/title").InnerText ?? "";
                            PageEntity urlElement = new PageEntity(title, scrapeLink, pubdlication);
                            TableOperation insertOp = TableOperation.InsertOrReplace(urlElement);
                            StorageManager.GetTable().Execute(insertOp);

                            HtmlNodeCollection href = webpage.DocumentNode.SelectNodes("//a[@href]");
                            if (href != null)
                            {
                                foreach (HtmlNode linkNode in href)
                                {
                                    string templink = linkNode.Attributes["href"].Value;
                                    if (templink.StartsWith("//"))
                                    {
                                        templink = "http:" + templink;
                                    }
                                    else if (templink.StartsWith("/"))
                                    {
                                        templink = "http://" + currentUri.Host + templink;
                                    }
                                    if(!HtmlSet.Contains(templink))
                                    {
                                        HtmlSet.Add(templink);
                                        CloudQueueMessage message1 = new CloudQueueMessage(templink);
                                        StorageManager.HTMLQueue().AddMessageAsync(message1);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (!HtmlSet.Contains(htmllink.AsString))
                            {
                                HtmlSet.Add(htmllink.AsString);
                                ErrorMessage urlerrorElement = new ErrorMessage(htmllink.AsString, e.Message);
                                TableOperation insertOp = TableOperation.InsertOrReplace(urlerrorElement);
                                StorageManager.ErrorTable().Execute(insertOp);
                            }
                        }
                    }
                }
                StorageManager.HTMLQueue().DeleteMessage(htmllink);
            }
        }
        public void GetPerfCounters()
        {
            PerformanceCounter mem = new PerformanceCounter("Memory", "Available MBytes", null);
            PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float perfCounterValue = cpu.NextValue();

            //Thread has to sleep for at least 1 sec for accurate value.
            System.Threading.Thread.Sleep(1000);

            perfCounterValue = cpu.NextValue();
            CpuUsage = (int) perfCounterValue;
            RamAvailable = (int) mem.NextValue();

            PerformanceCounterEntity counter = new PerformanceCounterEntity(CpuUsage, RamAvailable);
            TableOperation insertOp = TableOperation.InsertOrReplace(counter);
            StorageManager.PerformanceCounterTable().Execute(insertOp);
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                
                await Task.Delay(1000);
            }
        }
    }
}









