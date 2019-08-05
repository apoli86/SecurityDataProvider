using NHibernate;
using SecurityDataProvider.Entities;
using System.Collections.Generic;

namespace SecurityDataProvider.Services
{
    public interface ISecurityService
    {
        void AddSecurityList(ISession session, IList<Security> securityList);
        Security GetSecurityBySymbol(ISession session, string symbol);
        IList<Security> GetSecurityListByRequestDate(ISession session, System.DateTime requestDate);
    }
}