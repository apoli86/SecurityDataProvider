using System;

namespace PortfolioSecurity.Entities
{
    public class Portfolio
    {
        public virtual long PortfolioId { get; set; }
        public virtual DateTime CreateDate { get; set; }
        //public virtual IList<PortfolioNavDate> PortfolioNavDateList { get; set; }
    }
}
