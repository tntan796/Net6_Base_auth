namespace DNBase.Common.Constants
{
    public static class LoaiThaoTac
    {
        public const string Add = "add";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Accept = "accept";
        public const string Cancel = "cancel";
    }

    public static class AppsettingConstants
    {
        public const string MAIN_CONNECT_STRING = "Default";
        public const string UPLOAD_FOLDER = "AppSettings:UPLOAD_FOLDER";
        public const string VALIDATE_LDAP = "ValidateLDAP";
        public const int MAX_VALUES_FILE = 20971520;
    }

    public static class MessageResponseCommon
    {
        public const string SUCCESSFULL = "Lấy dữ liệu thành công";
        public const string CREATE_SUCCESSFULL = "Thêm mới thành công";
        public const string UPDATE_SUCCESSFULL = "Cập nhật thành công";
        public const string DELETE_SUCCESSFULL = "Xóa thành công";
        public const string ACCEPT_SUCCESSFULL = "Phê duyệt thành công";
        public const string CANCEL_SUCCESSFULL = "Từ chối thành công";
        public const string UPDATE_FAIL = "Cập nhật không thành công";
        public const string CREATE_FAIL = "Thêm mới không thành công";
        public const string DELETE_FAIL = "Xóa không thành công";
        public const string ACCEPT_FAILL = "Phê duyệt không thành công";
        public const string CANCEL_FAIL = "Từ chối không thành công";
    }

    public static class MessageValidateCommon
    {
        public const string DuplicateCode = "Trùng mã code";
        public const string KhongCoQuyen = "Không có quyền";
        public const string NotFound = "Không tồn tại bản ghi";
        public const string Required = "Trường {0} là bắt buộc";
        public const string Invalid = "Trường {0} không hợp lệ";
        public const string MaxLength = "Trường {0} không được vượt quá {1} ký tự";
        public const string MinLength = "Trường {0} không được ngắn hơn {1} ký tự";
        public const string MessageUploadFile = "Tệp tải lên không được quá 20MB";
    }

    public static class Claims
    {
        public const string UserId = nameof(UserId);
        public const string ProfileViewModel = nameof(ProfileViewModel);
        public const string Permissions = nameof(Permissions);
    }
}