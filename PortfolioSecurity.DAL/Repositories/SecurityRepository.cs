using NHibernate;
using PortfolioSecurity.Entities;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public class SecurityRepository : RepositoryBase<Security>, ISecurityRepository
    {
        public IList<Security> GetSecurityList(ISession session)
        {
            return session.QueryOver<Security>().List();
        }
    }
}
