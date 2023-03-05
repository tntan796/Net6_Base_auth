using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_VaiTro_NguoiSuDung : AuditedEntity
    {
        public Guid? NguoiDungId { get; set; }

        public Guid? VaiTroId { get; set; }
    }
}