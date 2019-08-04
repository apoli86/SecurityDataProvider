using System;

namespace SecurityDataProvider.Entities.Requests
{
    public class SecurityListRequest : MessageBase
    {
        public DateTime RequestDate { get; set; }
    }
}
