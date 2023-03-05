using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DNBase.DataLayer.EF.Entities
{
    [Index(nameof(Code), IsUnique = true, Name = "IX_CategoryItemCode")]
    public class CategoryItem : AuditedEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Guid? ParentId { get; set; }
        public int Order { get; set; } = 0;
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}