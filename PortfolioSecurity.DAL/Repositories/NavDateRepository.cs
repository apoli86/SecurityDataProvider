using NHibernate;
using PortfolioSecurity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortfolioSecurity.DAL.Repositories
{
    public class NavDateRepository : RepositoryBase<NavDate>, INavDateRepository
    {
        public NavDate GetNavDate(ISession session, DateTime navDate)
        {
            var currentNavDate = session.QueryOver<NavDate>().Where(x => x.Date == navDate).List().SingleOrDefault();

            return currentNavDate;
        }

        public IList<NavDate> GetNavDateByRefreshSecurityStaticDataStatus(ISession session, string refreshSecurityStaticDataStatus)
        {
            return session.QueryOver<NavDate>().Where(x => x.RefreshSecurityStaticDataStatus == refreshSecurityStaticDataStatus).List();
        }
    }
}
