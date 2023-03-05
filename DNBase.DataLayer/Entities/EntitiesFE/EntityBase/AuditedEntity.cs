using System;

namespace DNBase.DataLayer.EF.Entities
{
    public abstract class AuditedEntity : AuditedEntity<Guid>
    {
    }

    public abstract class AuditedEntity<TKey> : Entity<TKey>, IAuditedEntity where TKey : IEquatable<TKey>
    {
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public Boolean? IsDeleted { get; set; }
    }

    public interface IAuditedEntity
    {
        DateTime? CreatedAt { get; set; }
        Guid? CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }
        Boolean? IsDeleted { get; set; }
    }
}