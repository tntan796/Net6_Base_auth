using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        #region "Chuyển đổi chuỗi tìm kiếm"

        public static string ChuyenDoiChuoiTimKiem(this string input)
        {
            StringBuilder builder = new StringBuilder();
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var listTu = input.Split();  //Lấy danh sách từ của chuỗi nhập vào
            foreach (var tu in listTu)
            {
                string tuLower = tu.ToLower().Trim();                                   //Chuyển về chữ thường
                string tuLowerKhongDau = tuLower.Normalize(NormalizationForm.FormD);    //Chuyển đổi thành không dấu
                if (!KiemTraTuCoDau(tuLower)) //Nếu không có dấu thì mới thực hiện chuyển đổi.Nếu có dấu thì giữ nguyên từ
                {
                    int index = 0;
                    foreach (var chTuLowerKhongDau in tuLowerKhongDau)
                    {
                        if (CharUnicodeInfo.GetUnicodeCategory(chTuLowerKhongDau) != UnicodeCategory.NonSpacingMark)
                        {
                            //Chỗ này Replace thêm phát nữa để xử lý trường hợp: nhập là "ê, â, ă, ư, ơ,..." thì không tìm kiếm các từ "e a, u, o".
                            //VD: nhập từ "hiên" thì không hiển thị từ "hien" mà chỉ hiển thị từ "hiên" và "hiện, hiển, hiến"
                            string kyTuConvert = ReplaceCharSet(chTuLowerKhongDau.ToString()).Replace(chTuLowerKhongDau, tuLower[index]);
                            builder.Append(kyTuConvert);
                            index++;
                        }
                    }
                    builder.Append(" ");
                }
                else
                {
                    builder.Append(tuLower + " ");
                }
            }
            return EscapeWildcards(builder.ToString().Trim());
        }

        public static readonly string nguyenAmCoDau = "áàạảãấầậẩẫắằặẳẵéèẹẻẽếềệểễóòọỏõốồộổỗớờợởỡúùụủũứừựửữíìịỉĩýỳỵỷỹ";

        public static bool KiemTraTuCoDau(string input)
        {
            bool result = false;
            var listChar = input.ToCharArray();
            foreach (var kyTu in listChar)
            {
                if (nguyenAmCoDau.Contains(kyTu.ToString()))
                {
                    return true;
                }
            }
            return result;
        }

        public static string ReplaceCharSet(string input)
        {
            switch (input)
            {
                case "a":
                    return "[a\x00e0ả\x00e3\x00e1ạăằẳẵắặ\x00e2ầẩẫấậ]";

                case "e":
                    return "[e\x00e8ẻẽ\x00e9ẹ\x00eaềểễếệ]";

                case "i":
                    return "[i\x00ecỉĩ\x00edị]";

                case "o":
                    return "[o\x00f2ỏ\x00f5\x00f3ọ\x00f4ồổỗốộơờởỡớợ]";

                case "u":
                    return "[u\x00f9ủũ\x00faụưừửữứự]";

                case "y":
                    return "[yỳỷỹ\x00fdỵ]";

                case "d":
                    return "[dđ]";
                    //case "đ":
                    //    return "[dđ]";
            }
            return input;
        }

        public static string EscapeWildcards(string input)
        {
            return input.Trim().Replace("%", "[%]").Replace("_", "[_]");
        }
        #endregion

        #region "Common string"
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static string ToUnsignString(this string input)
        {
            input = input.Trim();
            for (int i = 0x20; i < 0x30; i++)
            {
                input = input.Replace(((char)i).ToString(), " ");
            }
            input = input.Replace(".", "-");
            input = input.Replace(" ", "-");
            input = input.Replace(",", "-");
            input = input.Replace(";", "-");
            input = input.Replace(":", "-");
            input = input.Replace("  ", "-");
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string str = input.Normalize(NormalizationForm.FormD);
            string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
            while (str2.IndexOf("?") >= 0)
            {
                str2 = str2.Remove(str2.IndexOf("?"), 1);
            }
            while (str2.Contains("--"))
            {
                str2 = str2.Replace("--", "-").ToLower();
            }
            return str2;
        }

        public static string EncryptPlainTextToCipherText(string PlainText, string SecurityKey)
        {
            byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);

            SHA256Managed objMD5CryptoService = new SHA256Managed();
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();

            var objTripleDESCryptoService = new AesCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
            objTripleDESCryptoService.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string DecryptCipherTextToPlainText(string CipherText, string SecurityKey)
        {
            byte[] toEncryptArray = Convert.FromBase64String(CipherText);
            SHA256Managed objMD5CryptoService = new SHA256Managed();

            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();

            var objTripleDESCryptoService = new AesCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            objTripleDESCryptoService.Clear();

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        private static Dictionary<string, string> _dayOfWeekDic = new Dictionary<string, string>
        {
            { "Monday", "Thứ 2"},
            { "Tuesday", "Thứ 3" },
            { "Wednesday", "Thứ 4"},
            { "Thursday", "Thứ 5"},
            { "Friday", "Thứ 6" },
            { "Saturday", "Thứ 7"},
            { "Sunday", "Chủ nhật"}
        };

        public static string GetLevel(string _dayOfWeekId)
        {
            return _dayOfWeekDic[_dayOfWeekId];
        }

        private static Dictionary<int, string> _monthInYear = new Dictionary<int, string>
        {
            { 1, "Tháng 1"},
            { 2, "Tháng 2" },
            { 3, "Tháng 3"},
            { 4, "Tháng 4"},
            { 5, "Tháng 5" },
            { 6, "Tháng 6"},
            { 7, "Tháng 7"},
            { 8, "Tháng 8"},
            { 9, "Tháng 9" },
            { 10, "Tháng 10"},
            { 11, "Tháng 11"},
            { 12, "Tháng 12"}
        };

        public static string GetMonth(int _monthInYearInt)
        {
            return _monthInYear[_monthInYearInt];
        }
        #endregion
    }
}