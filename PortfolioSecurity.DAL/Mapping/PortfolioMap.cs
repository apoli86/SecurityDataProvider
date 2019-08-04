using FluentNHibernate.Mapping;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.DAL.Mapping
{
    public class PortfolioMap : ClassMap<Portfolio>
    {
        public PortfolioMap()
        {
            Id(x => x.PortfolioId);
            Map(x => x.CreateDate);
            //HasMany(x => x.PortfolioNavDateList).KeyColumn(nameof(Portfolio.PortfolioId)).Inverse();
        }
    }
}
