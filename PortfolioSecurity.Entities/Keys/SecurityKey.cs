using System.Collections.Generic;

namespace PortfolioSecurity.Entities.Keys
{
    public class SecurityKey
    {
        private readonly string symbol;
        private readonly string name;
        private readonly string currency;

        public SecurityKey(string symbol, string name, string currency)
        {
            this.symbol = symbol?.ToUpperInvariant();
            this.name = name?.ToUpperInvariant();
            this.currency = currency?.ToUpperInvariant();
        }

        public override bool Equals(object obj)
        {
            return obj is SecurityKey key &&
                   symbol == key.symbol &&
                   name == key.name &&
                   currency == key.currency;
        }

        public override int GetHashCode()
        {
            var hashCode = 1314448749;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(symbol);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(currency);
            return hashCode;
        }

        public override string ToString()
        {
            return $"Symbol: {symbol}, Currency: {currency}, Name: {name}";
        }
    }
}
