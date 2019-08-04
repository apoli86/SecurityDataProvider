using System;

namespace PortfolioSecurity.Entities
{
    public class PortfolioSecurity
    {
        public virtual long PortfolioSecurityId { get; set; }
        public virtual Portfolio Portfolio { get; set; }
        public virtual Security Security { get; set; }
        public virtual DateTime CreateDate { get; set; }
    }
}
