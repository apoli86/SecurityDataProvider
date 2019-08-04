using NHibernate;
using NHibernate.Criterion;
using SecurityDataProvider.Entities;

namespace SecurityDataProvider.DAL.Repositories
{
    public class RequestRepository : RepositoryBase<Request>, IRequestRepository
    {
        public Request GetLastRequestByType(ISession session, string requestType)
        {
            return session.QueryOver<Request>().Where(r => r.RequestType == requestType).OrderBy(r => r.RequestDate).Desc().Take(1).SingleOrDefault();

        }

        public Request GetRequestByPayLoad(ISession session, string requestType, string payload)
        {
            return session.QueryOver<Request>().Where(r => r.RequestType == requestType && r.RequestPayload == payload).OrderBy(r => r.RequestDate).Desc().Take(1).SingleOrDefault();
        }
    }
}
