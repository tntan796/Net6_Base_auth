using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_Quyen : AuditedEntity
    {
        public string TenQuyen { get; set; }

        public string MaQuyen { get; set; }

        public string MoTa { get; set; }
    }
}