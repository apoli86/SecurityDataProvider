using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public interface IPortfolioSecurityRepository
    {
        IList<Entities.PortfolioSecurity> GetPortfolioSecurityList(ISession session, Portfolio portfolio);
    }
}