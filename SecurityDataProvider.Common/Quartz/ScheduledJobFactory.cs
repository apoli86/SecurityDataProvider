using NLog;
using Quartz;
using Quartz.Spi;
using System;

namespace SecurityDataProvider.Common.Quartz
{
    public class ScheduledJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;

        public ScheduledJobFactory(IServiceProvider serviceProvider, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            logger.Log(LogLevel.Info, $"Creating new instance of Job {bundle.JobDetail.JobType}...");

            try
            {
                return serviceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Error while creating new instance of Job {bundle.JobDetail.JobType}: {ex.Message}");
                throw;
            }
        }

        public void ReturnJob(IJob job)
        {
            logger.Log(LogLevel.Info, $"Disponsing instance of Job {job.GetType()}");

            var disposable = job as IDisposable;
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();

                    logger.Log(LogLevel.Info, $"Instance of Job {job.GetType()} has been disposed");
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex, $"Error while disposing new instance of Job {job.GetType()}: {ex.Message}");
                }
            }
        }
    }
}
