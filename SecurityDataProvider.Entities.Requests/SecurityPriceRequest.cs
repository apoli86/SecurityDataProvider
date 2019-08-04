using System;

namespace SecurityDataProvider.Entities.Requests
{
    public class SecurityPriceRequest : MessageBase
    {
        public string Symbol { get; set; }
        public DateTime NavDate { get; set; }
    }
}
