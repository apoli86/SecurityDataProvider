using FluentNHibernate.Mapping;
using SecurityDataProvider.Entities;

namespace SecurityDataProvider.DAL.Mapping
{
    public class SecurityMap : ClassMap<Security>
    {
        public SecurityMap()
        {
            Id(x => x.SecurityId);
            Map(x => x.RequestDate);
            Map(x => x.Currency);
            Map(x => x.Name);
            Map(x => x.Symbol);
            Map(x => x.CreateDate);
        }
    }
}
