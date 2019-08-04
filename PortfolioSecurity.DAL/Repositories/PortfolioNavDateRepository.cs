using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public class PortfolioNavDateRepository : RepositoryBase<PortfolioNavDate>, IPortfolioNavDateRepository
    {
        public IList<PortfolioNavDate> GetPortfolioNavDateListByNavDate(ISession session, NavDate navDate)
        {
            return session.QueryOver<PortfolioNavDate>()
                          .Fetch(SelectMode.ChildFetch, x => x.NavDate)
                          .Fetch(SelectMode.ChildFetch, x => x.Portfolio)
                          .Where(p => p.NavDate.NavDateId == navDate.NavDateId).List();
        }
    }
}
