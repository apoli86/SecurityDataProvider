using NHibernate;
using PortfolioSecurity.Entities;
using System;
using System.Collections.Generic;

namespace PortfolioSecurity.DAL.Repositories
{
    public interface INavDateRepository
    {
        NavDate GetNavDate(ISession session, DateTime navDate);
        void SaveOrUpdate(ISession session, NavDate obj);
        void Update(IStatelessSession session, NavDate obj);
        IList<NavDate> GetNavDateByRefreshSecurityStaticDataStatus(ISession session, string refreshSecurityStaticDataStatus);
    }
}