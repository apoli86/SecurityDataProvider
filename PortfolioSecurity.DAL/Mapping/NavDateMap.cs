using FluentNHibernate.Mapping;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.DAL.Mapping
{
    public class NavDateMap : ClassMap<NavDate>
    {
        public NavDateMap()
        {
            Id(x => x.NavDateId);
            Map(x => x.Date);
            Map(x => x.RefreshSecurityStaticDataStatus);
        }
    }
}
