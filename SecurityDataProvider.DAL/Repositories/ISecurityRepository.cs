using NHibernate;
using SecurityDataProvider.Entities;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.DAL.Repositories
{
    public interface ISecurityRepository
    {
        void SaveOrUpdate(ISession session, Security security);
        void Insert(IStatelessSession session, Security security);
        IList<Security> GetSecurityListByRequestDate(ISession session, DateTime requestDate);
        Security GetLastSecurityBySymbol(ISession session, string symbol);
    }
}