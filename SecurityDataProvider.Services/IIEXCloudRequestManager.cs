using SecurityDataProvider.Entities.Requests;
using System.Collections.Generic;

namespace SecurityDataProvider.Batch.Services
{
    public interface IIEXCloudRequestManager
    {
        Entities.Requests.SymbolPrice GetSymbolPrice(string symbol);
        IEnumerable<Symbol> GetSymbolList();
    }
}