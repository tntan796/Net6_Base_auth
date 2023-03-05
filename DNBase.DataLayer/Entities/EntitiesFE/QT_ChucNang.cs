using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_ChucNang : AuditedEntity
    {
        public Guid? CapChaId { get; set; }

        public string TenChucNang { get; set; }

        public string MaChucNang { get; set; }

        public string MoTa { get; set; }
    }
}