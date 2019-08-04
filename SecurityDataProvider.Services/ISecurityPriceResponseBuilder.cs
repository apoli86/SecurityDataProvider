using SecurityDataProvider.Entities;
using SecurityDataProvider.Entities.Requests;

namespace SecurityDataProvider.Services
{
    public interface ISecurityPriceResponseBuilder
    {
        SecurityPriceResponse BuildSecurityPriceResponse(SecurityPrice securityPrice);
    }
}