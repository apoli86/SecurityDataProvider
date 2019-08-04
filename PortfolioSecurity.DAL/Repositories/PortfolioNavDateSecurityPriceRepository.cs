using NHibernate;
using PortfolioSecurity.Entities;
using System;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public class PortfolioNavDateSecurityPriceRepository : RepositoryBase<PortfolioNavDateSecurityPrice>, IPortfolioNavDateSecurityPriceRepository
    {
        public IList<PortfolioNavDateSecurityPrice> GetPortfolioNavDateSecurityPriceByNavDateSymbol(ISession session, DateTime navDate, string symbol)
        {
            PortfolioNavDate pnd = null;
            Entities.PortfolioSecurity ps = null;
            NavDate nd = null;
            Security sd = null;
            Portfolio pt = null;

            return session.QueryOver<PortfolioNavDateSecurityPrice>()
                          .Fetch(SelectMode.ChildFetch, p => p.PortfolioNavDate)
                          .Fetch(SelectMode.ChildFetch, p => p.PortfolioSecurity)
                          .JoinAlias(p => p.PortfolioNavDate, () => pnd).JoinAlias(() => pnd.NavDate, () => nd).JoinAlias(() => pnd.Portfolio, () => pt)
                          .JoinAlias(p => p.PortfolioSecurity, () => ps).JoinAlias(() => ps.Security, () => sd)
                          .Where(() => nd.Date == navDate.Date)
                          .And(() => sd.Symbol == symbol)
                          .List();
        }

        public IList<PortfolioNavDateSecurityPrice> GetPortfolioNavDateSecurityPriceListByPortfolioNavDate(ISession session, PortfolioNavDate portfolioNavDate)
        {
            return session.QueryOver<PortfolioNavDateSecurityPrice>()
                          .Fetch(SelectMode.ChildFetch, p => p.PortfolioNavDate)
                          .Fetch(SelectMode.ChildFetch, p => p.PortfolioSecurity)
                          .Where(p => p.PortfolioNavDate.PortfolioNavDateId == portfolioNavDate.PortfolioNavDateId)
                          .List();
        }
    }
}
