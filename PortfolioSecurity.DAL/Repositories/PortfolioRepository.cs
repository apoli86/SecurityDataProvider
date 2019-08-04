using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public class PortfolioRepository : RepositoryBase<Portfolio>, IPortfolioRepository
    {
        public IList<Portfolio> GetPortfolioList(ISession session)
        {
            return session.QueryOver<Portfolio>()
                          .List();
        }
    }
}
