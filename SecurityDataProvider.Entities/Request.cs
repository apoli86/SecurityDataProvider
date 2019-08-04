using System;

namespace SecurityDataProvider.Entities
{
    public class Request
    {
        public virtual long RequestId { get; set; }
        public virtual string RequestType { get; set; }
        public virtual string RequestPayload { get; set; }
        public virtual DateTime RequestDate { get; set; }
        public virtual string RequestStatus { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public virtual DateTime UpdateDate { get; set; }
    }
}
