using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class AppLog : AuditedEntity
    {
        public string RequestPath { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
    }
}
