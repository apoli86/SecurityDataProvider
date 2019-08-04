using NHibernate;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.Services
{
    public interface INavDateService
    {
        NavDate CreateNavDateIfNotExists(ISession session);
        void FixStatusOfPreviousNavDate(ISession session, NavDate currentNavDate);
        NavDate GetCurrentNavDate(ISession session);
        void UpdateNavDate(ISession session, NavDate navDate);
        void UpdateNavDate(IStatelessSession session, NavDate navDate);
    }
}