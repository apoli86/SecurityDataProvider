using NHibernate;
using SecurityDataProvider.Entities;

namespace SecurityDataProvider.Services
{
    public interface ISecurityPriceService
    {
        SecurityPrice GetSecurityPriceBySymbol(ISession session, string symbol, System.DateTime navDate);
        SecurityPrice SaveSecurityPrice(ISession session, SecurityPrice newSecurityPrice);
    }
}