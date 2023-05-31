using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPGiaitriviet.Utils
{
    public class SmsTemp
    {
        public static string HHTD_VI = "Hoạt huyết T-Đình";
        public static string HHTD_EN = "Hoat huyet T-Dinh";
        public static string KTCD_VI = "Khớp tọa chi đan";
        public static string KTCD_EN = "Khop toa chi dan";
        public static string SUPERMAN = "Superman";
        public static string VVG_VI = "Viên vai gáy";
        public static string VVG_EN = "Vien vai gay";
        public static string ACTIVE_NOTVALID(string chanel)
        {
            string mtReturn = "Ma the khong ton tai. Xin vui long thu lai. Lien he 1800.1716 de duoc ho tro. Web http://quatang.duocbanhat.vn";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Mã thẻ không tồn tại. Xin vui lòng thử lại. Liên hệ 1800.1716 để được hỗ trợ";
            }
            return mtReturn;
        }

        //
        //public static string ACTIVE_OVER_QUOTA(string chanel)
        //{
        //    string mtReturn = "Tich diem khong khong thanh cong, A/c da tich luy qua han muc cua chuong trinh, moi so dien thoai duoc tich toi da 18 hop/thang. Hotline 1800.1716.";
        //    if ("WEB".Equals(chanel))
        //    {
        //        mtReturn = "Tích điểm không thành công. Anh/Chị đã tích lũy quá hạn mức của chương trình, mỗi số điện thoại được tích tối đa 18 hộp/tháng. Hotline 1800.1716.";
        //    }
        //    return mtReturn;
        //}
        public static string ACTIVE(string s1, string s2, string s3, string s4, string chanel)
        {
            string mtReturn = string.Format("A/C da tich luy {0} hop {1} chinh hang. Hay mua them {2} hop de duoc tang 1 hop. Hotline 1800.1716. http://quatang.duocbanhat.vn", s1, s2, s3);
            if ("WEB".Equals(chanel))
            {
                mtReturn = string.Format("Anh chị đã tích lũy {0} hộp {1} chính hãng. Hãy mua thêm {2} hộp để được tặng 1 hộp. Hotline 1800.1716.", s1, s4, s3); ;
            }
            return mtReturn;
            
        }

        public static string OVER_QUOTA(string chanel)
        {
            string mtReturn = "Tich diem khong thanh cong, A/c da tich luy qua han muc cua chuong trinh, moi so dien thoai duoc tich toi da 36 hop trong vong 6 thang. Hotline 1800.1716.";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Tích điểm không thành công, Anh/Chị đã tích lũy quá hạn mức của chương trình, mỗi số điện thoại được tích tối đa 36 hộp trên mỗi sản phẩm trong vòng 6 tháng. Hotline 1800.1716.";
            }
            return mtReturn;
        }

        public static string OVER_QUOTA_IN_MONTH(string chanel)
        {
            string mtReturn = "Tich diem khong thanh cong, A/c da tich luy qua han muc cua chuong trinh, moi so dien thoai duoc tich toi da 6 hop trong 1 thang. Hotline 1800.1716";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Tích điểm không thành công, Anh/Chị đã tích lũy quá hạn mức của chương trình, mỗi số điện thoại được tích tối đa 6 hộp trên mỗi sản phẩm trong vòng 1 tháng. Hotline 1800.1716.";
            }
            return mtReturn;

        }

        public static string ACTIVE(string s1, string s2, string s3, int? s4, string chanel)
        {
            string mtReturn = string.Format("Chuc mung ban co {0} diem va duoc tang {2} hop {1}. Chung toi se lien he voi ban de gui qua tang trong thoi gian som nhat. Hotline 1800.1716.", s1, s2, s4);
            if ("WEB".Equals(chanel))
            {
                mtReturn = string.Format("Chúc mừng bạn có {0} điểm và được tặng {2} hộp {1}. Chúng tôi sẽ liên hệ với bạn để gửi quà tặng trong thời gian sớm nhất. Hotline 1800.1716.", s1, s3, s4); ;
            }
            return mtReturn;
            
        }
        public static string ACTIVATED(string chanel)
        {
            string mtReturn = "Ma the cao da duoc su dung. Xin vui long thu lai. Lien he 1800.1716 de duoc ho tro. Web http://quatang.duocbanhat.vn";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Mã thẻ cào đã được sử dụng. Xin vui lòng thử lại. Liên hệ 1800.1716 để được hỗ trợ.";
            }
            return mtReturn;
            
        }

        public static string SUPERMAN_SUPPERSEND(string chanel)
        {
            string mtReturn = "Chuong trinh tich diem doi qua san pham SuperMan da tam dung tu ngay 16/10/2021. Hotline: 18001716";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Chương trình tích điểm đổi quà sản phẩm SuperMan đã tạm dừng từ ngày 16/10/2021. Liên hệ 1800.1716 để được hỗ trợ.";
            }
            return mtReturn;

        }

        public static string KTCD_SUPPERSEND(string chanel)
        {
            string mtReturn = "Chuong trinh tich diem doi qua san pham Khop toa chi dan da tam dung tu ngay 23/12/2021. Hotline: 18001716";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Chương trình tích điểm đổi quà sản phẩm Khớp tọa chi đan đã tạm dừng từ ngày 23/12/2021. Liên hệ 1800.1716 để được hỗ trợ.";
            }
            return mtReturn;

        }

        //
        public static string TRACUU_NOTVALID(string chanel)
        {
            string mtReturn = "SDT cua A/C chua tham gia CTKM, hay mua Hoat huyet T-dinh, Superman, Khop toa chi dan, Vien vai gay de nhan nhieu qua tang hap dan. Hotline 1800.1716.";
            if ("WEB".Equals(chanel))
            {
                mtReturn = "Số điện thoại của Anh/Chị chưa tham gia CTKM, hãy mua Hoạt Huyết T-Đình, Superman, Khớp tọa chi đan, Viên vai gáy để nhận nhiều quà tặng hấp dẫn. Hotline 1800.1716.";
            }
            return mtReturn;
            
        }

        public static string TRACUU_VALID(string s1, string s2, string s3, string s4, string chanel)
        {

            string mtReturn =  string.Format("A/C da tich luy {0} hop Hoat huyet T-Dinh chinh hang, {1} hop Superman, {2} hop Khop toa chi dan, {3} hop Vien vai gay. Hotline 1800.1716. http://quatang.duocbanhat.vn", s1, s2, s3, s4); 
            
            if ("WEB".Equals(chanel))
            {
                mtReturn = string.Format("Anh chị đã tích lũy {0} hộp Hoạt huyết T-Đình chính hãng, {1} hộp Superman, {2} hộp Khớp tọa chi đan, {3} hộp Viên vai gáy. Hotline 1800.1716.", s1, s2, s3, s4);
            }
            return mtReturn;
            
        }

        public static string SYNTAX_INVALID(string chanel)
        {

            string mtReturn = string.Format("A/C da nhan sai cu phap. Xin vui long thu lai. Lien he 1800.1716 de duoc ho tro. Web http://quatang.duocbanhat.vn");
           
            return mtReturn;

        }

    }
}