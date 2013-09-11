using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChavpMonitorMachine
{
    using Chavp.Monitors;
    using EasyNetQ;
    using EasyNetQ.Topology;
    using Quartz;

    [DisallowConcurrentExecution]
    public class MachineMonitorJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {

            var username = context.MergedJobDataMap["username"] as string;
            IExchange exchange = context.MergedJobDataMap["IExchange"] as IExchange;
            IAdvancedPublishChannel boxPublishChannel = context.MergedJobDataMap["IAdvancedPublishChannel"] as IAdvancedPublishChannel;
            MachineInfo machineInfo = context.MergedJobDataMap["MachineInfo"] as MachineInfo;

            machineInfo.Update();

            Console.WriteLine("Execute: " + DateTime.Now + ", CpuUsage: " + machineInfo.CpuUsage);

            var message = new Message<MachineInfo>(machineInfo);
            message.Properties.UserId = username;
            boxPublishChannel.Publish<MachineInfo>(exchange, "info", message);

        }
    }
}
