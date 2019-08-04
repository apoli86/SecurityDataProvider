using Quartz;
using System;

namespace SecurityDataProvider.Common.Quartz
{
    public static class SchedulerExtensions
    {
        public static ISchedulerFluent ScheduleJob<T>(this IScheduler scheduler) where T : IJob
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            var jobName = typeof(T).FullName;

            var job = JobBuilder.Create<T>()
                .WithIdentity(jobName, jobName)
                .StoreDurably()
                .Build();

            return new SchedulerFluent(job, scheduler);
        }

        public static void WaitUntilStarted(this IScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            scheduler.Start().Wait();
        }
    }

    public class SchedulerFluent : ISchedulerFluent
    {
        private readonly IJobDetail jobDetail;
        private readonly IScheduler scheduler;

        public SchedulerFluent(IJobDetail jobDetail, IScheduler scheduler)
        {
            this.jobDetail = jobDetail;
            this.scheduler = scheduler;
        }

        public void Every(TimeSpan interval)
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobDetail.JobType.FullName}.trigger", $"{jobDetail.JobType.FullName}.trigger")
                .StartNow()
                .WithSimpleSchedule(scheduleBuilder =>
                    scheduleBuilder
                        .WithInterval(interval)
                        .RepeatForever())
                        .ForJob(jobDetail)
                .Build();

            scheduler.ScheduleJob(jobDetail, trigger).Wait();
        }
    }

    public interface ISchedulerFluent
    {
        void Every(TimeSpan interval);
    }
}
