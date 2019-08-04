using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public class PortfolioSecurityRepository : IPortfolioSecurityRepository
    {
        public IList<Entities.PortfolioSecurity> GetPortfolioSecurityList(ISession session, Portfolio portfolio)
        {
            return session.QueryOver<Entities.PortfolioSecurity>()
                          .Fetch(SelectMode.ChildFetch, p => p.Portfolio)
                          .Fetch(SelectMode.ChildFetch, p => p.Security)
                          .Where(p => p.Portfolio.PortfolioId == portfolio.PortfolioId)
                          .List();

        }
    }
}
