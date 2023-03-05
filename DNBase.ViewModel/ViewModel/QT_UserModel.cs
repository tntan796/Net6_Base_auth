using System;
using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public class UserCreateRequestModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public List<Guid> listRole { get; set; } = new List<Guid>();
    }
    public class UserRespondModel
    {
        public string HoTen { get; set; }
        public string CMND { get; set; }
        public string SDT { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public List<string> DanhSachVaiTro { get; set; } = new List<string>();
        public List<string> DanhSachQuyen{ get; set; } = new List<string>();
    }
}