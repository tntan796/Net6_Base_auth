using System;

namespace DNBase.ViewModel
{
    public class QT_QuyenRequestModel
    {
        public Guid? Id { get; set; }
        public string TenQuyen { get; set; }
        public string MaQuyen { get; set; }
        public string MoTa { get; set; }
    }
    public class QT_QuyenRespondModel
    {
        public Guid? Id { get; set; }
        public string TenQuyen { get; set; }
        public string MaQuyen { get; set; }
        public string MoTa { get; set; }
    }
}