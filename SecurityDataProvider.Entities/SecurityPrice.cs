using System;

namespace SecurityDataProvider.Entities
{
    public class SecurityPrice
    {
        public virtual long SecurityPriceId { get; set; }
        public virtual Security Security { get; set; }
        public virtual DateTime NavDate { get; set; }
        public virtual decimal OpenPrice { get; set; }
        public virtual decimal ClosePrice { get; set; }
        public virtual DateTime CreateDate { get; set; }
    }
}
