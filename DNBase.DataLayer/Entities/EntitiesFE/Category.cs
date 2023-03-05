using Microsoft.EntityFrameworkCore;

namespace DNBase.DataLayer.EF.Entities
{
    [Index(nameof(Code), IsUnique = true, Name = "IX_CategoryCode")]
    public class Category : AuditedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int Order { get; set; } = 0;
    }
}