using NHibernate;
using SecurityDataProvider.Entities;

namespace SecurityDataProvider.DAL.Repositories
{
    public interface IRequestRepository
    {
        Request GetLastRequestByType(ISession session, string requestType);
        Request GetRequestByPayLoad(ISession session, string requestType, string payload);
        void SaveOrUpdate(ISession session, Request request);
    }
}