using System;

namespace SecurityDataProvider.Entities.Requests
{
    public class SecurityPriceResponse : MessageBase
    {
        public string Symbol { get; set; }
        public DateTime NavDate { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public string Currency { get; set; }
    }
}
