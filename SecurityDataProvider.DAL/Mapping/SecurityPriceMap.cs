using FluentNHibernate.Mapping;
using SecurityDataProvider.Entities;

namespace SecurityDataProvider.DAL.Mapping
{
    public class SecurityPriceMap : ClassMap<SecurityPrice>
    {
        public SecurityPriceMap()
        {
            Id(x => x.SecurityPriceId);
            References(x => x.Security).Column(nameof(Security.SecurityId)).ForeignKey().Not.Nullable();
            Map(x => x.NavDate);
            Map(x => x.OpenPrice);
            Map(x => x.ClosePrice);
            Map(x => x.CreateDate);
        }
    }
}
