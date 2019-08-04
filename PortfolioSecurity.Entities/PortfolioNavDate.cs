using System;

namespace PortfolioSecurity.Entities
{
    public class PortfolioNavDate
    {
        public virtual long PortfolioNavDateId { get; set; }
        public virtual NavDate NavDate { get; set; }
        public virtual Portfolio Portfolio { get; set; }
        public virtual DateTime CreateDate { get; set; }
    }
}
