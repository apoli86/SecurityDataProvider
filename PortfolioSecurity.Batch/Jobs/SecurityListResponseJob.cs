using Newtonsoft.Json;
using NHibernate;
using NLog;
using PortfolioSecurity.DAL.Repositories;
using PortfolioSecurity.Entities;
using PortfolioSecurity.Entities.Enum;
using PortfolioSecurity.Entities.Keys;
using PortfolioSecurity.Services;
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
    public class SecurityListResponseJob : IJob, IDisposable
    {
        private readonly ISessionFactory dbSessionFactory;
        private readonly INavDateService navDateService;
        private readonly ISecurityRepository securityRepository;
        private readonly IConnectionFactory queueConnectionFactory;
        private readonly ILogger logger;

        private CancellationTokenSource cancellationTokenSource;

        public SecurityListResponseJob(ISessionFactory dbSessionFactory, INavDateService navDateService, ISecurityRepository securityRepository, IConnectionFactory queueConnectionFactory, ILogger logger)
        {
            this.dbSessionFactory = dbSessionFactory;
            this.navDateService = navDateService;
            this.securityRepository = securityRepository;
            this.queueConnectionFactory = queueConnectionFactory;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.Log(LogLevel.Info, "Begin");

            try
            {
                using (var connection = queueConnectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.CreateQueueIfNotExists(Queues.SecurityListResponseQueue);

                    cancellationTokenSource = new CancellationTokenSource();

                    await channel.BindConsumer(ProcessResponse)
                                 .WithCancellationToken(cancellationTokenSource)
                                 .OnQueue(Queues.SecurityListResponseQueue)
                                 .WithManualAck();
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, ex.Message);
            }

            logger.Log(LogLevel.Info, "End");
        }

        private void ProcessResponse(IModel channel, BasicDeliverEventArgs e)
        {
            try
            {
                logger.Log(LogLevel.Info, "Processing SecurityListResponse");

                var response = e.GetMessage<SecurityListResponse>();

                logger.Log(LogLevel.Info, $"[SecurityListResponse] ErrorMessage: {response.ErrorMessage}, SecurityList count: {response.SecurityList?.Count}, RequestDate: {response.RequestDate}, MessageId: {response.MessageId}");
                
                ILookup<SecurityKey, SecurityDataProvider.Entities.Requests.Security> securityFromResponseLookup = GetSecurityLookupFromResponse(response.SecurityList);

                using (var session = dbSessionFactory.OpenSession())
                {
                    NavDate currentNavDate = navDateService.GetCurrentNavDate(session);

                    if (currentNavDate == null)
                    {
                        return;
                    }

                    if (DateTime.Today != response.RequestDate.Date)
                    {
                        logger.Log(LogLevel.Warn, $"[SecurityListResponse] RequestDate: {response.RequestDate} will be ignored because it is not related to today");

                        channel.SendAck(e.DeliveryTag);
                        return;
                    }

                    if (currentNavDate.RefreshSecurityStaticDataStatus == RefreshSecurityStaticDataStatus.Done)
                    {
                        logger.Log(LogLevel.Warn, $"[SecurityListResponse] RequestDate: {response.RequestDate} will be ignored because Securities have been already updated");

                        channel.SendAck(e.DeliveryTag);
                        return;
                    }

                    IDictionary<SecurityKey, Entities.Security> securityFromDbDictionary = GetSecurityDictionaryFromDb(session);

                    IList<SecurityKey> securityKeyFromResponseList = securityFromResponseLookup.Select(x => x.Key).ToList();
                    IList<SecurityKey> newOrUpdatedSecurityList = securityKeyFromResponseList.Except(securityFromDbDictionary.Keys).ToList();

                    IDictionary<SecurityKey, Entities.Security> securityFromDbBySymbolDictionary = securityFromDbDictionary.Values.ToDictionary(x => new SecurityKey(x.Symbol, string.Empty, x.Currency));

                    int newSecurityCount = 0;
                    int updateSecurityCount = 0;

                    using (var statelessSession = dbSessionFactory.OpenStatelessSession())
                    using (var transaction = statelessSession.BeginTransaction())
                    {
                        foreach (SecurityKey securityKey in newOrUpdatedSecurityList)
                        {
                            SecurityDataProvider.Entities.Requests.Security securityResponse = securityFromResponseLookup[securityKey].FirstOrDefault();
                            Entities.Security securityDb;

                            SecurityKey securityKeyWithoutName = new SecurityKey(securityResponse.Symbol?.ToUpper(), string.Empty, securityResponse.Symbol?.ToUpper());

                            if (!securityFromDbBySymbolDictionary.TryGetValue(securityKeyWithoutName, out securityDb))
                            {
                                securityDb = new Entities.Security()
                                {
                                    Symbol = securityResponse.Symbol?.ToUpper(),
                                    Name = securityResponse.Name,
                                    Currency = securityResponse.Currency,
                                    CreateDate = DateTime.Now,
                                    UpdateDate = DateTime.Now,
                                };

                                newSecurityCount++;
                            }
                            else
                            {
                                securityDb.Name = securityResponse.Name;
                                securityDb.Currency = securityResponse.Currency;

                                updateSecurityCount++;
                            }

                            securityRepository.Insert(statelessSession, securityDb);

                        }

                        currentNavDate.RefreshSecurityStaticDataStatus = RefreshSecurityStaticDataStatus.Done;
                        navDateService.UpdateNavDate(statelessSession, currentNavDate);

                        transaction.Commit();

                        logger.Log(LogLevel.Info, $"[SecurityListResponse] RequestDate {response.RequestDate} Processed: new {newSecurityCount}, updated {updateSecurityCount}");
                    }

                    channel.SendAck(e.DeliveryTag);
                }
            }
            catch (JsonException jsonException)
            {
                logger.Log(LogLevel.Error, jsonException, jsonException.Message);
                channel.SendAck(e.DeliveryTag);

            }
            catch (Exception exception)
            {
                logger.Log(LogLevel.Error, exception, exception.Message);
                channel.SendNack(e.DeliveryTag);
            }
        }

        private IDictionary<SecurityKey, Entities.Security> GetSecurityDictionaryFromDb(ISession session)
        {
            var securityList = securityRepository.GetSecurityList(session);

            return securityList.ToDictionary(x => new SecurityKey(x.Symbol, x.Name, x.Currency));
        }

        private ILookup<SecurityKey, SecurityDataProvider.Entities.Requests.Security> GetSecurityLookupFromResponse(List<SecurityDataProvider.Entities.Requests.Security> securityList)
        {
            securityList = securityList ?? new List<SecurityDataProvider.Entities.Requests.Security>();

            return securityList?.ToLookup(x => new SecurityKey(x.Symbol, x.Name, x.Currency));
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
