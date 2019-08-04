using System;

namespace PortfolioSecurity.Entities
{
    public class PortfolioNavDateSecurityPrice
    {
        public virtual long PortfolioNavDateSecurityPriceId { get; set; }
        public virtual PortfolioNavDate PortfolioNavDate { get; set; }
        public virtual PortfolioSecurity PortfolioSecurity { get; set; }
        public virtual string Currency { get; set; }
        public virtual string PriceStatus { get; set; }
        public virtual decimal? OpenPrice { get; set; }
        public virtual decimal? ClosePrice { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public virtual DateTime UpdateDate { get; set; }
    }
}
