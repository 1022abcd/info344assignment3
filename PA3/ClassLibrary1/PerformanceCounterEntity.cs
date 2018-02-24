using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class PerformanceCounterEntity : TableEntity
    {
        public PerformanceCounterEntity()
        {

        }
        public PerformanceCounterEntity(int cpuusage,  int ramavailable)
        {
            this.PartitionKey = "PerformanceCounter";
            this.RowKey = "1";
            this.CPUUsage = cpuusage;
            this.RamAvailable = ramavailable;
        }

        public int CPUUsage { get; set; }
        public int RamAvailable { get; set; }
    }
}
