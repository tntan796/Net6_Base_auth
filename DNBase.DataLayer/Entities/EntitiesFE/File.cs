using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class Files : AuditedEntity
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public decimal Size { get; set; }
        public string Path { get; set; }
        public Guid? EntityId { get; set; }
        public string EntityName { get; set; }
        public string FileTypeUpload { get; set; }
        public bool IsPrivate { get; set; }
    }
}