using Newtonsoft.Json;
using NHibernate;
using NLog;
using Quartz;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SecurityDataProvider.Batch.Services;
using SecurityDataProvider.Common.RabbitMq;
using SecurityDataProvider.Entities;
using SecurityDataProvider.Entities.Requests;
using SecurityDataProvider.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityDataProvider.Batch.Jobs
{
    [DisallowConcurrentExecution]
    public class SecurityPriceResponseJob : IJob, IDisposable
    {
        private readonly ILogger logger;
        private readonly IConnectionFactory connectionFactory;
        private readonly ISessionFactory sessionFactory;
        private readonly IRequestService requestService;
        private readonly ISecurityService securityService;
        private readonly IIEXCloudRequestManager iexCloudRequestManager;
        private readonly ISecurityPriceBuilder securityPriceBuilder;
        private readonly ISecurityPriceService securityPriceService;
        private readonly ISecurityPriceResponseBuilder securityPriceResponseBuilder;

        private CancellationTokenSource cancellationTokenSource;

        public SecurityPriceResponseJob(ILogger logger, IConnectionFactory connectionFactory, ISessionFactory sessionFactory, IRequestService requestService, ISecurityService securityService, IIEXCloudRequestManager iexCloudRequestManager, ISecurityPriceBuilder securityPriceBuilder, ISecurityPriceService securityPriceService, ISecurityPriceResponseBuilder securityPriceResponseBuilder)
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.sessionFactory = sessionFactory;
            this.requestService = requestService;
            this.securityService = securityService;
            this.iexCloudRequestManager = iexCloudRequestManager;
            this.securityPriceBuilder = securityPriceBuilder;
            this.securityPriceService = securityPriceService;
            this.securityPriceResponseBuilder = securityPriceResponseBuilder;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var connection = connectionFactory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.CreateQueueIfNotExists(Queues.SecurityPriceRequestQueue);

                        this.cancellationTokenSource = new CancellationTokenSource();

                        await channel.BindConsumer(ProcessSecurityPriceRequest)
                                     .WithCancellationToken(cancellationTokenSource)
                                     .OnQueue(Queues.SecurityPriceRequestQueue)
                                     .WithManualAck();

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, ex.Message);
            }
        }

        private void ProcessSecurityPriceRequest(IModel channel, BasicDeliverEventArgs msg)
        {
            try
            {
                logger.Log(LogLevel.Info, $"Processing SecurityPriceRequest {msg.BasicProperties.MessageId}");

                using (var session = sessionFactory.OpenSession())
                {
                    var requestFromQueue = msg.GetMessage<SecurityPriceRequest>();

                    string requestFromQueueSymbol = requestFromQueue.Symbol?.ToUpper();
                    DateTime requestFromQueueNavDate = requestFromQueue.NavDate;
                    string requestFromQueueMessageId = requestFromQueue.MessageId;

                    string securityPriceRequestLog = $"[SecurityPriceRequest: NavDate {requestFromQueueNavDate}, Symbol {requestFromQueueSymbol}, MessageId {requestFromQueueMessageId}]";

                    logger.Log(LogLevel.Info, securityPriceRequestLog);

                    Request requestFromDb = requestService.GetOrCreateRequest(session, requestFromQueue, RequestType.SecurityPriceRequest, requestFromQueueNavDate);

                    Entities.Security security = securityService.GetSecurityBySymbol(session, requestFromQueueSymbol);

                    if (security == null)
                    {
                        logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: missing Security with Symbol {requestFromQueueSymbol}");

                        requestService.SetRequestStatus(session, requestFromDb, RequestStatus.Error);
                        channel.SendNack(msg.DeliveryTag);

                        return;
                    }


                    if (requestFromDb.RequestStatus == RequestStatus.Done)
                    {
                        logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: already processed with success, see Request {requestFromDb.RequestId}");

                        logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: retrieving SecurityPrice with same nav date and symbol");

                        SecurityPrice securityPrice = securityPriceService.GetSecurityPriceBySymbol(session, requestFromQueueSymbol, requestFromQueueNavDate.Date);

                        if (securityPrice != null)
                        {
                            logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: sending SecurityPrice Id: {securityPrice.SecurityPriceId}, NavDate: {securityPrice.NavDate}, Symbol: {securityPrice.Security.Symbol}, OpenPrice: {securityPrice.OpenPrice}, ClosePrice: {securityPrice.ClosePrice}");

                            PublishSecurityPrice(channel, securityPrice, requestFromQueue.MessageId);
                            channel.SendAck(msg.DeliveryTag);

                            logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: sent SecurityPrice Id: {securityPrice.SecurityPriceId}, NavDate: {securityPrice.NavDate}, Symbol: {securityPrice.Security.Symbol}, OpenPrice: {securityPrice.OpenPrice}, ClosePrice: {securityPrice.ClosePrice}");

                            return;
                        }

                        requestService.SetRequestStatus(session, requestFromDb, RequestStatus.Error);
                        channel.SendNack(msg.DeliveryTag);

                        return;
                    }

                    logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: retrieving SecurityPrice from IEXCloud for NavDate {requestFromQueueNavDate}");
                    SymbolPrice symbolPrice = iexCloudRequestManager.GetSymbolPrice(requestFromQueueSymbol);
                    logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: SecurityPrice from IEXCloud retrived - Symbol: {symbolPrice.symbol}, OpenPrice: {symbolPrice.open}, ClosePrice: {symbolPrice.close} ");

                    logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: Saving SecurityPrice from IEXCloud");

                    SecurityPrice dbSecurityPrice = SaveSecurityPrice(session, symbolPrice, security, requestFromQueueNavDate);

                    logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: SecurityPrice Id: {dbSecurityPrice.SecurityPriceId} saved");

                    logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: Sending SecurityPrice Id: {dbSecurityPrice.SecurityPriceId}, NavDate: {dbSecurityPrice.NavDate}, Symbol: {dbSecurityPrice.Security.Symbol}, OpenPrice: {dbSecurityPrice.OpenPrice}, ClosePrice: {dbSecurityPrice.ClosePrice} on {Queues.SecurityPriceResponseQueue}");

                    PublishSecurityPrice(channel, dbSecurityPrice, requestFromQueue.MessageId);

                    logger.Log(LogLevel.Info, $"{securityPriceRequestLog}: SecurityPrice Id: {dbSecurityPrice.SecurityPriceId} on {Queues.SecurityPriceResponseQueue} sent");

                    requestService.SetRequestStatus(session, requestFromDb, RequestStatus.Done);

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

        private Entities.SecurityPrice SaveSecurityPrice(ISession session, SymbolPrice symbolPrice, Entities.Security security, DateTime navDate)
        {
            SecurityPrice securityPrice = securityPriceBuilder.BuildSecurityPrice(security, symbolPrice.open, symbolPrice.close, navDate);

            return securityPriceService.SaveSecurityPrice(session, securityPrice);
        }

        private void PublishSecurityPrice(IModel channel, Entities.SecurityPrice securityPrice, string requestMessageId)
        {
            var securityPriceResponse = securityPriceResponseBuilder.BuildSecurityPriceResponse(securityPrice);
            securityPriceResponse.MessageId = requestMessageId;

            channel.PublishMessageOnQueue(securityPriceResponse, Queues.SecurityPriceResponseQueue);
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
