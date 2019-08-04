using System;
using System.Collections.Generic;

namespace SecurityDataProvider.Entities.Requests
{
    public class SecurityListResponse : MessageBase
    {
        public SecurityListResponse()
        {
            SecurityList = new List<Security>();
            ErrorMessage = string.Empty;
        }
        public List<Security> SecurityList { get; set; }
        public DateTime RequestDate { get; set; }
        public string ErrorMessage { get; set; }
    }
}
