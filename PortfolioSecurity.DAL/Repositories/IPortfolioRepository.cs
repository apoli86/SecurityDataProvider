using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public interface IPortfolioRepository
    {
        IList<Portfolio> GetPortfolioList(ISession session);
    }
}