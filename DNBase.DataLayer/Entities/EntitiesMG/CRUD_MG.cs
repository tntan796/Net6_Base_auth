using System;

namespace DNBase.DataLayer.EF.Mongo.Entities
{
    public class CRUD_MG : AuditedEntity
    {
        public string Title { get; set; }

        public string Code { get; set; }

        public string Content { get; set; }

        public int Status { get; set; }
    }
}
