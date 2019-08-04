using System;

namespace PortfolioSecurity.Entities
{
    public class NavDate
    {
        public virtual long NavDateId { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string RefreshSecurityStaticDataStatus { get; set; }
        //public virtual IList<PortfolioNavDate> PortfolioNavDateList { get; set; }

    }
}
