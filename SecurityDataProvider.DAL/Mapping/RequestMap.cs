using FluentNHibernate.Mapping;
using SecurityDataProvider.Entities;

namespace SecurityDataProvider.DAL.Mapping
{
    public class RequestMap : ClassMap<Request>
    {
        public RequestMap()
        {
            Id(x => x.RequestId);
            Map(x => x.RequestDate);
            Map(x => x.RequestPayload);
            Map(x => x.RequestStatus);
            Map(x => x.RequestType);
            Map(x => x.CreateDate);
            Map(x => x.UpdateDate);
        }
    }
}
