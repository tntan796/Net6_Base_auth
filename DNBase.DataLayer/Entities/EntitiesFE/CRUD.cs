using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class CRUD : AuditedEntity
    {
        public string Title { get; set; }

        public string Code { get; set; }

        public string Content { get; set; }

        public int? Status { get; set; }
    }

}
