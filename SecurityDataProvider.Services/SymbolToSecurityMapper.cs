using SecurityDataProvider.Entities.Requests;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.Services
{
    public class SymbolToSecurityMapper : ISymbolToSecurityMapper
    {
        public SymbolToSecurityMapper()
        {

        }

        public IList<Entities.Security> MapSymbolToSecurity(IEnumerable<Symbol> symbolList)
        {
            IList<Entities.Security> securityList = new List<Entities.Security>();

            foreach (Symbol symbol in symbolList)
            {
                Entities.Security security = new Entities.Security()
                {
                    Symbol = symbol.symbol,
                    Name = symbol.name,
                    Currency = symbol.currency,
                    CreateDate = DateTime.Now,
                    RequestDate = DateTime.Today
                };

                securityList.Add(security);
            }

            return securityList;
        }
    }
}
