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
    using Quartz.Impl;

    class Program
    {
        static void Main(string[] args)
        {
            string rabbitMQBrokerHost = "localhost";
            string virtualHost = "machines";
            string username = "machine-1";
            string password = "123456789";

            string connectionString = string.Format(
                "host={0};virtualHost={1};username={2};password={3}",
                rabbitMQBrokerHost, virtualHost, username, password);

            using (IAdvancedBus bus = RabbitHutch.CreateBus(connectionString).Advanced)
            {
                IExchange exchange = bus.ExchangeDeclare("machine", EasyNetQ.Topology.ExchangeType.Fanout);
                IAdvancedPublishChannel boxPublishChannel = bus.OpenPublishChannel();
                MachineInfo machineInfo = new MachineInfo();

                var jobDataMap = new JobDataMap();
                jobDataMap.Add("username", username);
                jobDataMap.Add("IAdvancedBus", bus);
                jobDataMap.Add("IExchange", exchange);
                jobDataMap.Add("IAdvancedPublishChannel", boxPublishChannel);
                jobDataMap.Add("MachineInfo", machineInfo);

                ISchedulerFactory sf = new StdSchedulerFactory();
                IScheduler sched = sf.GetScheduler();

                // computer a time that is on the next round minute
                DateTimeOffset runTime = DateBuilder.EvenMinuteDate(DateTimeOffset.UtcNow);

                IJobDetail job = JobBuilder.Create<MachineMonitorJob>()
                    .UsingJobData(jobDataMap)
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(
                    x => x.WithInterval(TimeSpan.FromMilliseconds(1000)).RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                sched.ScheduleJob(job, trigger);

                // Start up the scheduler (nothing can actually run until the 
                // scheduler has been started)
                sched.Start();

                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();

                sched.Shutdown(true);
            }

        }
    }
}
