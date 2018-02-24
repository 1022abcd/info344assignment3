using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class StorageManager
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);

        public static CloudQueue LinkQueue()
        {
            CloudQueueClient clientQueue = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = clientQueue.GetQueueReference("linkqueue");
            queue.CreateIfNotExists();
            return queue;
        }

        public static CloudQueue HTMLQueue()
        {
            CloudQueueClient clientQueue = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = clientQueue.GetQueueReference("htmlqueue");
            queue.CreateIfNotExists();
            return queue;
        }

        public static CloudQueue CommandQueue()
        {
            CloudQueueClient clientQueue = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = clientQueue.GetQueueReference("commandqueue");
            queue.CreateIfNotExists();
            return queue;
        }

        public static CloudTable GetTable()
        {
            CloudTableClient clientTable = storageAccount.CreateCloudTableClient();
            CloudTable table = clientTable.GetTableReference("gettable");
            table.CreateIfNotExists();
            return table;
        }

        public static CloudTable ErrorTable()
        {
            CloudTableClient clientTable = storageAccount.CreateCloudTableClient();
            CloudTable table = clientTable.GetTableReference("errortable");
            table.CreateIfNotExists();
            return table;
        }

        public static CloudTable PerformanceCounterTable()
        {
            CloudTableClient clientTable = storageAccount.CreateCloudTableClient();
            CloudTable table = clientTable.GetTableReference("performancecountertable");
            table.CreateIfNotExists();
            return table;
        }
        
    }
}

