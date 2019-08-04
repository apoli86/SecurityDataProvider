using NHibernate;
using NLog;
using PortfolioSecurity.Entities;
using PortfolioSecurity.Services;
using Quartz;
using System;
using System.Threading.Tasks;

namespace PortfolioSecurity.Batch.Jobs
{
    [DisallowConcurrentExecution]
    public class NavDateJob : IJob
    {
        private readonly ISessionFactory dbSessionFactory;
        private readonly INavDateService navDateService;
        private readonly ILogger logger;
        public NavDateJob(ISessionFactory dbSessionFactory, INavDateService navDateService, ILogger logger)
        {
            this.dbSessionFactory = dbSessionFactory;
            this.navDateService = navDateService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.Log(LogLevel.Info, "Begin");

            try
            {
                using (var session = dbSessionFactory.OpenSession())
                {
                    NavDate currentNavDate = navDateService.CreateNavDateIfNotExists(session);

                    navDateService.FixStatusOfPreviousNavDate(session, currentNavDate);
                }
            }
            catch (Exception exception)
            {
                logger.Log(LogLevel.Error, exception, exception.Message);
            }

            logger.Log(LogLevel.Info, "End");

            await Task.CompletedTask;
        }
    }
}
