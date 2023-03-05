using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DNBase.DataLayer.EF.Entities
{
    public class AppUser : IdentityUser<Guid>, IEntity<Guid>, IAuditedEntity
    {
        public string GioiTinh { get; set; }
        public bool IsSystemUser { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public string ResetToken { get; set; }   //Dùng cho việc quên mật khẩu
        public DateTime? ResetTokenExpires { get; set; }
    }
}