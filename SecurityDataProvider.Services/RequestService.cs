using Newtonsoft.Json;
using NHibernate;
using SecurityDataProvider.Common.NewtonJson;
using SecurityDataProvider.DAL.Repositories;
using SecurityDataProvider.Entities;
using SecurityDataProvider.Entities.Requests;
using System;

namespace SecurityDataProvider.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository requestRepository;

        public RequestService(IRequestRepository requestRepository)
        {
            this.requestRepository = requestRepository;
        }

        public Request GetOrCreateRequest<T>(ISession session, T message, string requestType, DateTime requestDate) where T : MessageBase
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (requestType == null)
            {
                throw new ArgumentNullException(nameof(requestType));
            }

            string payload = Serialize(message);
            Request lastRequest = requestRepository.GetRequestByPayLoad(session, requestType, payload);

            if (lastRequest != null && lastRequest.RequestStatus != RequestStatus.Done)
            {
                lastRequest.RequestStatus = RequestStatus.InProgress;
                lastRequest.UpdateDate = DateTime.Now;
                requestRepository.SaveOrUpdate(session, lastRequest);
            }

            if (lastRequest == null)
            {
                lastRequest = new Request()
                {
                    RequestPayload = payload,
                    RequestDate = requestDate,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    RequestStatus = RequestStatus.InProgress,
                    RequestType = requestType
                };

                requestRepository.SaveOrUpdate(session, lastRequest);
            }

            return lastRequest;
        }

        public void SetRequestStatus(ISession session, Request request, string status)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            request.RequestStatus = status;
            request.UpdateDate = DateTime.Now;
            requestRepository.SaveOrUpdate(session, request);
        }

        public void UpdateRequest(ISession session, Request request)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            requestRepository.SaveOrUpdate(session, request);
        }

        private string Serialize<T>(T message) where T : MessageBase
        {
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            jsonResolver.IgnoreProperty(typeof(MessageBase), nameof(MessageBase.MessageId));

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = jsonResolver;

            return JsonConvert.SerializeObject(message, serializerSettings);
        }
    }
}
