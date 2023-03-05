using System;

namespace DNBase.DataLayer.EF.Mongo.Entities
{
    public abstract class AuditedEntity : AuditedEntity<string>
    {
    }

    public abstract class AuditedEntity<TKey> : Entity<TKey>, IAuditedEntity where TKey : IEquatable<TKey>
    {
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public Boolean? IsDeleted { get; set; } = false;
    }

    public interface IAuditedEntity
    {
        DateTime? CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedBy { get; set; }
        Boolean? IsDeleted { get; set; }
    }
}