using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QT_DanhSachAPI : AuditedEntity
    {
        public string InterfaceName { get; set; }

        public string ActionName { get; set; }

        public string Method { get; set; }
    }
}