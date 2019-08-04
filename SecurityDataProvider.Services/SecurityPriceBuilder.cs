using SecurityDataProvider.Entities;
using System;

namespace SecurityDataProvider.Services
{
    public class SecurityPriceBuilder : ISecurityPriceBuilder
    {
        public SecurityPrice BuildSecurityPrice(Security security, decimal openPrice, decimal closePrice, DateTime navDate)
        {
            return new SecurityPrice()
            {
                Security = security,
                OpenPrice = openPrice,
                ClosePrice = closePrice,
                NavDate = navDate,
                CreateDate = DateTime.Now
            };
        }
    }
}
