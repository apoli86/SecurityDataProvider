using SecurityDataProvider.Entities.Requests;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.Services
{
    public class SecurityListResponseBuilder : ISecurityListResponseBuilder
    {
        public SecurityListResponse BuildSecurityListResponse(IList<Entities.Security> securityList, DateTime requestDate)
        {
            SecurityListResponse response = new SecurityListResponse();
            response.RequestDate = requestDate;
            response.SecurityList = new List<Security>();

            foreach (Entities.Security security in securityList)
            {
                Security securityRequest = new Security()
                {
                    Symbol = security.Symbol,
                    Name = security.Name,
                    Currency = security.Currency
                };

                response.SecurityList.Add(securityRequest);
            }

            return response;
        }
    }
}
