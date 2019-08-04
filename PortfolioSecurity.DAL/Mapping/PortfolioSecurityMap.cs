using FluentNHibernate.Mapping;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.DAL.Mapping
{
    public class PortfolioSecurityMap : ClassMap<Entities.PortfolioSecurity>
    {
        public PortfolioSecurityMap()
        {
            Id(x => x.PortfolioSecurityId);
            Map(x => x.CreateDate);
            References(x => x.Security).Column(nameof(Security.SecurityId)).ForeignKey().Not.Nullable();
            References(x => x.Portfolio).Column(nameof(Portfolio.PortfolioId)).ForeignKey().Not.Nullable();
        }
    }
}
