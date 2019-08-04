using SecurityDataProvider.Entities.Requests;
using System;
using System.Collections.Generic;

namespace SecurityDataProvider.Services
{
    public interface ISecurityListResponseBuilder
    {
        SecurityListResponse BuildSecurityListResponse(IList<Entities.Security> securityList, DateTime requestDate);
    }
}