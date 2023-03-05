using Microsoft.AspNetCore.Identity;
using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
    }
}