using FluentNHibernate.Mapping;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.DAL.Mapping
{
    public class PortfolioNavDateSecurityPriceMap : ClassMap<PortfolioNavDateSecurityPrice>
    {
        public PortfolioNavDateSecurityPriceMap()
        {
            Id(x => x.PortfolioNavDateSecurityPriceId);
            References(x => x.PortfolioNavDate).Column(nameof(PortfolioNavDate.PortfolioNavDateId)).ForeignKey().Not.Nullable();
            References(x => x.PortfolioSecurity).Column(nameof(Entities.PortfolioSecurity.PortfolioSecurityId)).ForeignKey().Not.Nullable();
            Map(x => x.ClosePrice);
            Map(x => x.CreateDate);
            Map(x => x.OpenPrice);
            Map(x => x.PriceStatus);
            Map(x => x.UpdateDate);
            Map(x => x.Currency);
        }
    }
}
