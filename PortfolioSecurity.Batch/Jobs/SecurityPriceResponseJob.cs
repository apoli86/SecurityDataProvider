using Newtonsoft.Json;
using NHibernate;
using NLog;
using PortfolioSecurity.DAL.Repositories;
using PortfolioSecurity.Entities;
using PortfolioSecurity.Entities.Enum;
using Quartz;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SecurityDataProvider.Common.RabbitMq;
using SecurityDataProvider.Entities.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PortfolioSecurity.Batch.Jobs
{
    [DisallowConcurrentExecution]
    public class SecurityPriceResponseJob : IJob, IDisposable
    {
        private readonly ILogger logger;
        private readonly IConnectionFactory connectionFactory;
        private readonly ISessionFactory sessionFactory;
        private readonly IPortfolioNavDateSecurityPriceRepository portfolioNavDateSecurityPriceRepository;

        private CancellationTokenSource cancellationTokenSource;

        public SecurityPriceResponseJob(ILogger logger, IConnectionFactory connectionFactory, ISessionFactory sessionFactory, IPortfolioNavDateSecurityPriceRepository portfolioNavDateSecurityPriceRepository)
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.sessionFactory = sessionFactory;
            this.portfolioNavDateSecurityPriceRepository = portfolioNavDateSecurityPriceRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.Log(LogLevel.Info, "Begin");

            try
            {
                using (var connection = connectionFactory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.CreateQueueIfNotExists(Queues.SecurityPriceResponseQueue);

                        this.cancellationTokenSource = new CancellationTokenSource();

                        await channel.BindConsumer(ProcessSecurityPriceResponse)
                                     .WithCancellationToken(cancellationTokenSource)
                                     .OnQueue(Queues.SecurityPriceResponseQueue)
                                     .WithManualAck();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e, e.Message);
            }

            logger.Log(LogLevel.Info, "End");
        }

        private void ProcessSecurityPriceResponse(IModel channel, BasicDeliverEventArgs msg)
        {
            try
            {
                logger.Log(LogLevel.Info, "Processing SecurityPriceResponse");

                var response = msg.GetMessage<SecurityPriceResponse>();

                using (var session = sessionFactory.OpenSession())
                {
                    logger.Log(LogLevel.Info, $"[SecurityPriceResponse: Symbol {response.Symbol}, NavDate {response.NavDate.Date}, MessageId {response.MessageId}]: looking for existing PortfolioNavSecurityPrice on db with same symbol and nav date");
                    IList<PortfolioNavDateSecurityPrice> securityPriceList = portfolioNavDateSecurityPriceRepository.GetPortfolioNavDateSecurityPriceByNavDateSymbol(session, response.NavDate.Date, response.Symbol?.ToUpper());

                    if (!securityPriceList.Any())
                    {
                        logger.Log(LogLevel.Info, $"[SecurityPriceResponse: Symbol {response.Symbol}, NavDate {response.NavDate.Date}, MessageId {response.MessageId}]: no PortfolioNavSecurityPrice on db with same symbol and nav date");

                        channel.SendAck(msg.DeliveryTag);

                        return;
                    }

                    UpdateSecurityPricesWithReceivedPrices(session, response, securityPriceList);

                    channel.SendAck(msg.DeliveryTag);
                }
            }
            catch (JsonException jsonException)
            {
                logger.Log(LogLevel.Error, jsonException, jsonException.Message);
                channel.SendAck(msg.DeliveryTag);
            }
            catch (Exception exception)
            {
                logger.Log(LogLevel.Error, exception, exception.Message);
                channel.SendNack(msg.DeliveryTag);
            }
        }

        private void UpdateSecurityPricesWithReceivedPrices(ISession session, SecurityPriceResponse response, IList<PortfolioNavDateSecurityPrice> securityPriceList)
        {
            using (var transaction = session.BeginTransaction())
            {
                foreach (PortfolioNavDateSecurityPrice securityPrice in securityPriceList)
                {
                    if (securityPrice.PriceStatus == PriceStatus.Received)
                    {
                        continue;
                    }

                    securityPrice.OpenPrice = response.OpenPrice;
                    securityPrice.ClosePrice = response.ClosePrice;
                    securityPrice.PriceStatus = PriceStatus.Received;
                    securityPrice.UpdateDate = DateTime.Now;

                    logger.Log(LogLevel.Info, $"[SecurityPriceResponse: Symbol {response.Symbol}, NavDate {response.NavDate.Date}, MessageId {response.MessageId}]: updating [PortfolioNavSecurityPrice: Id {securityPrice.PortfolioNavDateSecurityPriceId}, PortfolioId {securityPrice.PortfolioNavDate.Portfolio.PortfolioId}, Symbol {securityPrice.PortfolioSecurity.Security.Symbol}, NavDate {securityPrice.PortfolioNavDate.NavDate.Date}] with Status {securityPrice.PriceStatus}, OpenPrice {securityPrice.OpenPrice}, ClosePrice {securityPrice.ClosePrice}");

                    portfolioNavDateSecurityPriceRepository.SaveOrUpdate(session, securityPrice);

                    logger.Log(LogLevel.Info, $"[SecurityPriceResponse: Symbol {response.Symbol}, NavDate {response.NavDate.Date}, MessageId {response.MessageId}]: updated [PortfolioNavSecurityPrice: Id {securityPrice.PortfolioNavDateSecurityPriceId}, PortfolioId {securityPrice.PortfolioNavDate.Portfolio.PortfolioId}, Symbol {securityPrice.PortfolioSecurity.Security.Symbol}, NavDate {securityPrice.PortfolioNavDate.NavDate.Date}]");
                }

                transaction.Commit();
            }
        }

        public void Dispose()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}
