using NHibernate;
using SecurityDataProvider.Entities;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.DAL.Repositories
{
    public interface ISecurityPriceRepository
    {
        IList<SecurityPrice> GetSecurityPriceList(ISession session, DateTime navDate);
        SecurityPrice GetSecurityPriceBySymbol(ISession session, string symbol, DateTime navDate);
        void SaveOrUpdate(ISession session, SecurityPrice securityPrice);
    }
}