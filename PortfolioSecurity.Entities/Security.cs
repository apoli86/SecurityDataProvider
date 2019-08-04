using System;

namespace PortfolioSecurity.Entities
{
    public class Security
    {
        public virtual long SecurityId { get; set; }
        public virtual string Symbol { get; set; }
        public virtual string Name { get; set; }
        public virtual string Currency { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public virtual DateTime UpdateDate { get; set; }
    }
}
