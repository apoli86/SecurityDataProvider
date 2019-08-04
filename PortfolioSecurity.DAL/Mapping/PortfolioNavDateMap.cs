using FluentNHibernate.Mapping;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.DAL.Mapping
{
    public class PortfolioNavDateMap : ClassMap<Entities.PortfolioNavDate>
    {
        public PortfolioNavDateMap()
        {
            Id(x => x.PortfolioNavDateId);
            References(x => x.NavDate).Column(nameof(NavDate.NavDateId)).ForeignKey().Not.Nullable();
            References(x => x.Portfolio).Column(nameof(Portfolio.PortfolioId)).ForeignKey().Not.Nullable();
            Map(x => x.CreateDate);
        }
    }
}
