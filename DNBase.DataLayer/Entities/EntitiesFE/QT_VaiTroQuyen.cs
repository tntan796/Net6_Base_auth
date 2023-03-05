using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_VaiTroQuyen : AuditedEntity
    {
        public Guid? VaiTroId { get; set; }

        public Guid? ChucNangId { get; set; }

        public Guid? QuyenId { get; set; }

    }
}