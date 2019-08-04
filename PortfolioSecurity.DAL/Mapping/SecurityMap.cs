using FluentNHibernate.Mapping;
using PortfolioSecurity.Entities;

namespace PortfolioSecurity.DAL.Mapping
{
    public class SecurityMap : ClassMap<Security>
    {
        public SecurityMap()
        {
            Id(x => x.SecurityId);
            Map(x => x.Symbol);
            Map(x => x.Name);
            Map(x => x.Currency);
            Map(x => x.CreateDate);
            Map(x => x.UpdateDate);

        }
    }
}
