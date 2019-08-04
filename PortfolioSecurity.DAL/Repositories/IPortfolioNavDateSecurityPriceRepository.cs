using NHibernate;
using PortfolioSecurity.Entities;
using System;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public interface IPortfolioNavDateSecurityPriceRepository
    {
        IList<PortfolioNavDateSecurityPrice> GetPortfolioNavDateSecurityPriceListByPortfolioNavDate(ISession session, PortfolioNavDate portfolioNavDate);
        void Insert(IStatelessSession session, PortfolioNavDateSecurityPrice portfolioNavDateSecurityPrice);
        void Update(IStatelessSession session, PortfolioNavDateSecurityPrice portfolioNavDateSecurityPrice);
        void SaveOrUpdate(ISession session, PortfolioNavDateSecurityPrice portfolioNavDateSecurityPrice);
        IList<PortfolioNavDateSecurityPrice> GetPortfolioNavDateSecurityPriceByNavDateSymbol(ISession session, DateTime navDate, string symbol);

    }
}