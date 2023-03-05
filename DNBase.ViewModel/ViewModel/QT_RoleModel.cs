using System;
using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public class RoleRequestModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ChucNangQuyenRequestModel> listChucNangQuyen{ get; set; } = new List<ChucNangQuyenRequestModel>();
    }
    public class ChucNangQuyenRequestModel
    {
        public Guid? ChucNangId { get; set; }
        public Guid? QuyenId { get; set; }
    }
    public class RoleRespondModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ChucNangQuyenRespondModel> listChucNangQuyen { get; set; } = new List<ChucNangQuyenRespondModel>();
    }

    public class ChucNangQuyenRespondModel
    {
        public Guid? ChucNangId { get; set; }
        public string MaChucNang { get; set; }
        public Guid? QuyenId { get; set; }
        public string MaQuyen { get; set; }
    }
}