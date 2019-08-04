using SecurityDataProvider.Entities;
using System;

namespace SecurityDataProvider.Services
{
    public interface ISecurityPriceBuilder
    {
        SecurityPrice BuildSecurityPrice(Security security, decimal openPrice, decimal closePrice, DateTime navDate);
    }
}