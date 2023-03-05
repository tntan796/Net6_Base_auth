using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_DonVi : AuditedEntity
    {
        public string TenDonVi { get; set; }

        public string MaDonVi { get; set; }

        public string GhiChu { get; set; }
    }
}