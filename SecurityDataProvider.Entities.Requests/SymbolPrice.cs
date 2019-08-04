using System;

namespace SecurityDataProvider.Entities.Requests
{
    public class SymbolPrice
    {
        public string symbol { get; set; }
        public DateTime date { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
    }
}