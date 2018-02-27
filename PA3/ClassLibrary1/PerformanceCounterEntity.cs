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
        public PerformanceCounterEntity(int cpuusage,  int ramavailable, string state)
        {
            this.PartitionKey = "PerformanceCounter";
            this.RowKey = "1";
            this.CPUUsage = cpuusage;
            this.RamAvailable = ramavailable;
            this.State = state;
            
        }

        public int CPUUsage { get; set; }
        public int RamAvailable { get; set; }
        public string State { get; set; }
    }
}
