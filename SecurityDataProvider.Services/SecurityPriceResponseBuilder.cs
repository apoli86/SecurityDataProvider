using SecurityDataProvider.Entities;
using SecurityDataProvider.Entities.Requests;

namespace SecurityDataProvider.Services
{
    public class SecurityPriceResponseBuilder : ISecurityPriceResponseBuilder
    {
        public SecurityPriceResponse BuildSecurityPriceResponse(SecurityPrice securityPrice)
        {
            var securityPriceResponse = new SecurityPriceResponse()
            {
                ClosePrice = securityPrice.ClosePrice,
                OpenPrice = securityPrice.OpenPrice,
                Currency = securityPrice.Security.Currency,
                NavDate = securityPrice.NavDate,
                Symbol = securityPrice.Security.Symbol
            };

            return securityPriceResponse;
        }
    }
}
