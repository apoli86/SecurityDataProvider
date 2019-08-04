using SecurityDataProvider.Entities.Requests;
using System.Collections.Generic;

namespace SecurityDataProvider.Services
{
    public interface ISymbolToSecurityMapper
    {
        IList<Entities.Security> MapSymbolToSecurity(IEnumerable<Symbol> symbolList);
    }
}