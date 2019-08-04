using NHibernate;
using SecurityDataProvider.Entities;
using SecurityDataProvider.Entities.Requests;
using System;

namespace SecurityDataProvider.Services
{
    public interface IRequestService
    {
        Request GetOrCreateRequest<T>(ISession session, T payload, string requestType, DateTime requestDate) where T : MessageBase;
        void UpdateRequest(ISession session, Request request);
        void SetRequestStatus(ISession session, Request request, string requestStatus);
    }
}