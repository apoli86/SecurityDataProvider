using NHibernate;
using SecurityDataProvider.Entities;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.DAL.Repositories
{
    public class SecurityRepository : RepositoryBase<Security>, ISecurityRepository
    {
        public IList<Security> GetSecurityListByRequestDate(ISession session, DateTime requestDate)
        {
            return session.QueryOver<Security>().Where(s => s.RequestDate == requestDate.Date).List();
        }

        public Security GetSecurityBySymbol(ISession session, string symbol, DateTime requestDate)
        {
            return session.QueryOver<Security>().Where(s => s.Symbol == symbol && s.RequestDate == requestDate).SingleOrDefault();
        }

        public void Insert(IStatelessSession session, Security security)
        {
            session.Insert(security);
        }
    }
}
