using NHibernate;
using NLog;
using PortfolioSecurity.Entities;
using PortfolioSecurity.Entities.Enum;
using PortfolioSecurity.Services;
using Quartz;
using RabbitMQ.Client;
using SecurityDataProvider.Common.RabbitMq;
using SecurityDataProvider.Entities.Requests;
using System;
using System.Threading.Tasks;

namespace PortfolioSecurity.Batch.Jobs
{
    [DisallowConcurrentExecution]
    public class SecurityListRequestJob : IJob
    {
        private readonly ISessionFactory dbSessionFactory;
        private readonly INavDateService navDateService;
        private readonly IConnectionFactory queueConnectionFactory;
        private readonly ILogger logger;

        public SecurityListRequestJob(ISessionFactory dbSessionFactory, INavDateService navDateService, IConnectionFactory queueConnectionFactory, ILogger logger)
        {
            this.dbSessionFactory = dbSessionFactory;
            this.navDateService = navDateService;
            this.queueConnectionFactory = queueConnectionFactory;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.Log(LogLevel.Info, "Begin");

            try
            {
                using (var session = dbSessionFactory.OpenSession())
                {
                    NavDate currentNavDate = navDateService.GetCurrentNavDate(session);

                    if (currentNavDate == null)
                    {
                        throw new Exception("Missing Current NavDate");
                    }

                    if (currentNavDate.RefreshSecurityStaticDataStatus != RefreshSecurityStaticDataStatus.Done)
                    {
                        PublishSecurityListRequest();

                        currentNavDate.RefreshSecurityStaticDataStatus = RefreshSecurityStaticDataStatus.InProgress;

                        navDateService.UpdateNavDate(session, currentNavDate);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Log(LogLevel.Error, exception, exception.Message);

                using (var session = dbSessionFactory.OpenSession())
                {
                    SetCurrentNavDateInError(session);
                }
            }

            logger.Log(LogLevel.Info, "End");

            await Task.CompletedTask;
        }

        private void PublishSecurityListRequest()
        {
            using (IConnection connection = queueConnectionFactory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.CreateQueueIfNotExists(Queues.SecurityListRequestQueue);

                    var message = new SecurityListRequest() { RequestDate = DateTime.Today, MessageId = Guid.NewGuid().ToString() };

                    channel.PublishMessageOnQueue(message, Queues.SecurityListRequestQueue);

                    logger.Log(LogLevel.Info, $"SecurityListRequest has been sent: RequestDate {message.RequestDate.Date}, MessageId {message.MessageId}");
                }
            }
        }

        private void SetCurrentNavDateInError(ISession session)
        {
            var currentNavDate = navDateService.GetCurrentNavDate(session);

            if (currentNavDate != null)
            {
                currentNavDate.RefreshSecurityStaticDataStatus = RefreshSecurityStaticDataStatus.Error;
                navDateService.UpdateNavDate(session, currentNavDate);
            }
        }
    }
}
