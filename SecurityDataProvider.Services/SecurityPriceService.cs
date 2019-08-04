using NHibernate;
using SecurityDataProvider.DAL.Repositories;
using SecurityDataProvider.Entities;
using System;

namespace SecurityDataProvider.Services
{
    public class SecurityPriceService : ISecurityPriceService
    {
        private readonly ISecurityPriceRepository securityPriceRepository;

        public SecurityPriceService(ISecurityPriceRepository securityPriceRepository)
        {
            this.securityPriceRepository = securityPriceRepository;
        }

        public SecurityPrice SaveSecurityPrice(ISession session, SecurityPrice newSecurityPrice)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (newSecurityPrice == null)
            {
                throw new ArgumentNullException(nameof(newSecurityPrice));
            }

            Entities.SecurityPrice existingSecurityPrice = securityPriceRepository.GetSecurityPriceBySymbol(session, newSecurityPrice.Security.Symbol, newSecurityPrice.NavDate);

            if (existingSecurityPrice == null)
            {
                securityPriceRepository.SaveOrUpdate(session, newSecurityPrice);

                return newSecurityPrice;
            }
            else
            {
                existingSecurityPrice.OpenPrice = newSecurityPrice.OpenPrice;
                existingSecurityPrice.ClosePrice = newSecurityPrice.ClosePrice;

                securityPriceRepository.SaveOrUpdate(session, existingSecurityPrice);

                return existingSecurityPrice;
            }
        }

        public SecurityPrice GetSecurityPriceBySymbol(ISession session, string symbol, DateTime navDate)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            SecurityPrice securityPrice = securityPriceRepository.GetSecurityPriceBySymbol(session, symbol.ToUpper(), navDate);

            return securityPrice;
        }
    }
}
