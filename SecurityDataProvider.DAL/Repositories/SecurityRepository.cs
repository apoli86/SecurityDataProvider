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

        public Security GetLastSecurityBySymbol(ISession session, string symbol)
        {
            return session.QueryOver<Security>().Where(s => s.Symbol == symbol).OrderBy(x => x.RequestDate).Desc.Take(1).SingleOrDefault();
        }

        public void Insert(IStatelessSession session, Security security)
        {
            session.Insert(security);
        }
    }
}
