using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_ChucNangQuyenAPI : AuditedEntity
    {
        public Guid? ChucNangId { get; set; }

        public Guid? QuyenId { get; set; }

        public Guid? ApiId { get; set; }
    }
}