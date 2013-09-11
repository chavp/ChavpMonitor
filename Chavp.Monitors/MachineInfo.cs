using System;
using System.Diagnostics;

namespace Chavp.Monitors
{
    public class MachineInfo
    {
        public string MachineName { get; set; }
        public DateTime MachineTime { get; set; }
        public float CpuUsage { get; set; }

        PerformanceCounter _cpuCounter;
        public MachineInfo()
        {
            MachineName = Environment.MachineName;

            _cpuCounter = new PerformanceCounter();
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";
        }

        public void Update()
        {
            MachineTime = DateTime.Now;
            CpuUsage = _cpuCounter.NextValue();
        }
    }
}
