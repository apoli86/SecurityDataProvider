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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityDataProvider.Batch.Jobs
{
    [DisallowConcurrentExecution]
    public class SecurityListResponseJob : IJob, IDisposable
    {
        private readonly ISessionFactory dbSessionFactory;
        private readonly IRequestService requestService;
        private readonly ISecurityService securityService;
        private readonly ISymbolToSecurityMapper symbolToSecurityMapper;
        private readonly ISecurityListResponseBuilder securityListResponseBuilder;
        private readonly IIEXCloudRequestManager securityDataProviderRequestManager;
        private readonly IConnectionFactory queueConnectionFactory;
        private readonly ILogger logger;

        private CancellationTokenSource cancellationTokenSource;

        public SecurityListResponseJob(ISessionFactory dbSessionFactory, IRequestService requestService, ISecurityService securityService, ISymbolToSecurityMapper symbolToSecurityMapper, ISecurityListResponseBuilder securityListResponseBuilder, IIEXCloudRequestManager securityDataProviderRequestManager, IConnectionFactory queueConnectionFactory, ILogger logger)
        {
            this.dbSessionFactory = dbSessionFactory;
            this.requestService = requestService;
            this.securityService = securityService;
            this.symbolToSecurityMapper = symbolToSecurityMapper;
            this.securityListResponseBuilder = securityListResponseBuilder;
            this.securityDataProviderRequestManager = securityDataProviderRequestManager;
            this.queueConnectionFactory = queueConnectionFactory;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var connection = queueConnectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.CreateQueueIfNotExists(Queues.SecurityListRequestQueue);

                    this.cancellationTokenSource = new CancellationTokenSource();

                    await channel.BindConsumer(ProcessResponse)
                                 .WithCancellationToken(cancellationTokenSource)
                                 .OnQueue(Queues.SecurityListRequestQueue)
                                 .WithManualAck();

                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e, e.Message);
            }
        }

        private void ProcessResponse(IModel channel, BasicDeliverEventArgs e)
        {
            try
            {
                logger.Log(LogLevel.Info, $"Processing SecurityListRequest");

                var requestFromQueue = e.GetMessage<SecurityListRequest>();

                logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]");

                using (var session = dbSessionFactory.OpenSession())
                {
                    Request requestFromDb = requestService.GetOrCreateRequest(session, requestFromQueue, RequestType.SecurityListRequest, DateTime.Today);

                    bool hasSameRequestDate = requestFromDb != null && requestFromDb.RequestDate == requestFromQueue.RequestDate.Date;
                    bool isRequestStatusDone = requestFromDb != null && requestFromDb.RequestStatus == RequestStatus.Done;
                    if (hasSameRequestDate && isRequestStatusDone)
                    {
                        logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: already processed with success, see Request {requestFromDb.RequestId}");

                        logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: retrieving Securities from db");

                        SecurityListResponse securityListResponse = BuildSecurityListResponse(session, requestFromQueue.RequestDate, requestFromQueue.MessageId);

                        logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: {securityListResponse.SecurityList?.Count} Securities retrieved");

                        logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: sending SecurityListResponse with {securityListResponse.SecurityList?.Count} Securities");

                        channel.PublishMessageOnQueue(securityListResponse, Queues.SecurityListResponseQueue);

                        logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: SecurityListResponse sent");

                        channel.SendAck(e.DeliveryTag);

                        return;
                    }

                    if (!hasSameRequestDate)
                    {
                        logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: Request from db is related to {requestFromDb.RequestDate}, no Securities available for {requestFromQueue.RequestDate}");

                        SecurityListResponse securityListResponse = new SecurityListResponse() { ErrorMessage = $"Securities at {requestFromQueue.RequestDate} not available", MessageId = requestFromQueue.MessageId };

                        channel.PublishMessageOnQueue(securityListResponse, Queues.SecurityListResponseQueue);

                        channel.SendAck(e.DeliveryTag);

                        return;
                    }

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: Retrieving Securities from IEXCloud");

                    IEnumerable<Symbol> symbolList = securityDataProviderRequestManager.GetSymbolList();

                    IList<Entities.Security> securityList = symbolToSecurityMapper.MapSymbolToSecurity(symbolList);

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: {securityList.Count} Securities from IEXCloud retrieved");

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: Saving {securityList.Count} Securities");

                    securityService.AddSecurityList(session, securityList);

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: {securityList.Count} Securities saved");

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: Creating SecurityListResponse");

                    SecurityListResponse response = BuildSecurityListResponse(session, requestFromQueue.RequestDate, requestFromQueue.MessageId);

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: SecurityListResponse created with {response.SecurityList?.Count} Securities");

                    channel.PublishMessageOnQueue(response, Queues.SecurityListResponseQueue);

                    logger.Log(LogLevel.Info, $"[SecurityListRequest: RequestDate {requestFromQueue.RequestDate}, MessageId {requestFromQueue.MessageId}]: SecurityListResponse published on queue {Queues.SecurityListResponseQueue}");

                    requestService.SetRequestStatus(session, requestFromDb, RequestStatus.Done);

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

        private SecurityListResponse BuildSecurityListResponse(ISession session, DateTime requestDate, string messageId)
        {
            IList<Entities.Security> securityList = securityService.GetSecurityListByRequestDate(session, requestDate);

            SecurityListResponse response = securityListResponseBuilder.BuildSecurityListResponse(securityList, requestDate);
            response.MessageId = messageId;

            return response;
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
