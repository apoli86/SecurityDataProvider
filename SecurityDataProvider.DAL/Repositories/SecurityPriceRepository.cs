using NHibernate;
using SecurityDataProvider.Entities;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.DAL.Repositories
{
    public class SecurityPriceRepository : RepositoryBase<SecurityPrice>, ISecurityPriceRepository
    {
        public IList<SecurityPrice> GetSecurityPriceList(ISession session, DateTime navDate)
        {
            return session.QueryOver<SecurityPrice>().Where(x => x.NavDate == navDate.Date).List();
        }

        public SecurityPrice GetSecurityPriceBySymbol(ISession session, string symbol, DateTime navDate)
        {
            return session.QueryOver<SecurityPrice>()
                          .Where(sp => sp.NavDate == navDate.Date)
                          .JoinQueryOver(s => s.Security).Where(s => s.Symbol == symbol)
                          .SingleOrDefault();
        }
    }
}
