using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public interface IPortfolioNavDateRepository
    {
        IList<PortfolioNavDate> GetPortfolioNavDateListByNavDate(ISession session, NavDate navDate);
        void SaveOrUpdate(ISession session, PortfolioNavDate portfolioNavDate);
    }
}