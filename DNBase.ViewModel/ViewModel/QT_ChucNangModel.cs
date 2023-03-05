using System;

namespace DNBase.ViewModel
{
    public class QT_ChucNangRequestModel
    {
        public Guid? Id { get; set; }

        public Guid? CapChaId { get; set; }

        public string TenChucNang { get; set; }

        public string MaChucNang { get; set; }

        public string MoTa { get; set; }
    }
    public class QT_ChucNangRespondModel
    {
        public Guid? Id { get; set; }

        public Guid? CapChaId { get; set; }

        public string TenChucNang { get; set; }

        public string MaChucNang { get; set; }

        public string MoTa { get; set; }
    }
}