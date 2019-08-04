namespace PortfolioSecurity.Entities.Enum
{
    public class PriceStatus
    {
        public const string ToBeRequested = nameof(ToBeRequested);
        public const string Requested = nameof(Requested);
        public const string Received = nameof(Received);
        public const string Expired = nameof(Expired);
        public const string InvalidNavDate = nameof(InvalidNavDate);
    }
}
