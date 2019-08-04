using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public interface ISecurityRepository
    {
        IList<Security> GetSecurityList(ISession session);
        void SaveOrUpdate(ISession session, Security obj);
        void Insert(IStatelessSession session, Security obj);
    }
}