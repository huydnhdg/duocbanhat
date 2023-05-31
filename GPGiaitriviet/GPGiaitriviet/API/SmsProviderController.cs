using GPGiaitriviet.Models;
using GPGiaitriviet.Utils;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace GP_Giaitriviet.API
{
    [RoutePrefix("api/sms")]
    public class SmsProviderController : ApiController
    {
        Logger log = LogManager.GetCurrentClassLogger();
        GiaitrivietEntities db = new GiaitrivietEntities();
        private readonly int NUMBER_OVER_QUOTA = 36;
        private readonly int NUMBER_OVER_QUOTA_IN_MONTH = 6;

        [Route("receive")]
        [HttpGet]
        public HttpResponseMessage Receive(string Command_Code, string User_ID, string Service_ID, string Request_ID, string Message)
        {
            // Lưu log file
            log.Info(string.Format("[MO] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, Message));
            User_ID = MyControl.formatUserId(User_ID, 0);
            // Lưu log vào DB
            var LogMO = new LogMO()
            {
                Command_Code = Command_Code,
                Phone = User_ID,
                Service_ID = Service_ID,
                Request_ID = Request_ID,
                Message = Message,
                Createdate = DateTime.Now
            };
            db.LogMOes.Add(LogMO);
            db.SaveChanges();
            long id = LogMO.ID;
            // Kết thúc ghi log vào DB

            string mt_trakhachhang = "";
            string category = "TC";
            string chanel = "SMS";
            int status = 0; // 0 - thành công, 1- thất bại, 2 - tra cứu, 3 - sai cú pháp, 4 - trùng mã thẻ, 5 - Quá hạn mức trong 6 tháng, 6- Quá hạn mức trong 1 tháng
            if (Request_ID.ToUpper().StartsWith("WEB"))
            {
                chanel = "WEB";
            }
            try
            {
                string[] words = Message.ToUpper().Split(' ');
                if (words.Length < 2)
                {
                    mt_trakhachhang = SmsTemp.SYNTAX_INVALID(chanel);
                    category = "SYNTAX_INVALID";
                    status = 3;
                }
                else if (words[1] == "TC")
                {
                    // Tra cứu thông tin sản phẩm, Trả về các giá trị trong bảng DB Customer
                    var customer1 = db.Customers.Where(a => a.Phone == User_ID).SingleOrDefault();
                    if (customer1 == null)
                    {
                        mt_trakhachhang = SmsTemp.TRACUU_NOTVALID(chanel);
                    }
                    else
                    {
                        mt_trakhachhang = SmsTemp.TRACUU_VALID(customer1.HHTD.ToString(), customer1.SUPERMAN.ToString(), customer1.KTCD.ToString(), customer1.VVG.ToString(), chanel);
                    }
                    status = 2;
                }
                else
                {
                    // Nếu gộp thông tin toàn bộ sản phẩm thành Code, đảm bảo khách hàng nhắn tin XXX XXX XXX vẫn OK
                    String Code = Message.Substring(Command_Code.Length + 1);
                    Code = Code.Replace(" ", "").ToUpper();
                    log.Info(string.Format("Phone = {0}, Code = {1}", User_ID, Code));
                    var codeGP = db.CodeGPs.Where(a => a.Code == Code.Trim()).SingleOrDefault();

                    if (codeGP == null)
                    {
                        mt_trakhachhang = SmsTemp.ACTIVE_NOTVALID(chanel);
                        status = 1;
                        category = "ERROR_CODE";
                    } else if (codeGP.Category.Equals("SUPERMAN"))
                    {
                        // ADD BY TRUNGVD 2021-10-15 Task = 1429825
                        mt_trakhachhang = SmsTemp.SUPERMAN_SUPPERSEND(chanel);
                        status = 1;
                    }
                    else if (codeGP.Category.Equals("KTCD"))
                    {
                        // ADD BY TRUNGVD 2021-12-23 Task = 1895873
                        mt_trakhachhang = SmsTemp.KTCD_SUPPERSEND(chanel);
                        status = 1;
                    }
                    else
                    {
                        category = codeGP.Category.ToUpper();
                        // Đã kích hoạt bảo hành trước đó
                        if (codeGP.Status == 1)
                        {
                            mt_trakhachhang = SmsTemp.ACTIVATED(chanel);
                            status = 4;
                        } 
                        else
                        {
                            // Tích lũy cho khách hàng
                            //codeGP.Status = 1;
                            //codeGP.Phone = User_ID;
                            //codeGP.Activedate = DateTime.Now;
                            //// Cập nhật thông tin khách hàng
                            //db.Entry(codeGP).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges(); // Thêm ngày 31/07/2021
                            // Kiểm tra xem đã có thông tin trong bảng DB customer hay chưa
                            // Check số lượng mã của Category
                            // var Codes = db.CodeGPs.Where(a => a.Category.Equals(category) && a.Phone.Equals(User_ID));
                            
                            //
                            var customer1 = db.Customers.Where(a => a.Phone == User_ID).SingleOrDefault();
                            if (customer1 == null)
                            {
                                // Tích lũy cho khách hàng
                                codeGP.Status = 1;
                                codeGP.Phone = User_ID;
                                codeGP.Activedate = DateTime.Now;
                                // Cập nhật thông tin khách hàng
                                db.Entry(codeGP).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges(); // Thêm ngày 31/07/2021
                                //Insert vào bảng Customer
                                Customer newCustomer = new Customer();
                                switch (category)
                                {
                                    case "HHTD":
                                        newCustomer = new Customer()
                                        {
                                            Phone = User_ID,
                                            HHTD = 1,
                                            KTCD = 0,
                                            SUPERMAN = 0,
                                            VVG = 0,
                                            Createdate = DateTime.Now,
                                            StartHHTD = DateTime.Now
                                        };
                                        mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.HHTD_EN, "5", SmsTemp.HHTD_VI, chanel);
                                        break;
                                    case "KTCD":
                                        newCustomer = new Customer()
                                        {
                                            Phone = User_ID,
                                            HHTD = 0,
                                            KTCD = 1,
                                            SUPERMAN = 0,
                                            VVG = 0,
                                            Createdate = DateTime.Now,
                                            StartKTCD = DateTime.Now
                                        };
                                        mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.KTCD_EN, "5", SmsTemp.KTCD_VI, chanel);
                                        break;
                                    case "SUPERMAN":
                                        newCustomer = new Customer()
                                        {
                                            Phone = User_ID,
                                            HHTD = 0,
                                            KTCD = 0,
                                            SUPERMAN = 1,
                                            VVG = 0,
                                            Createdate = DateTime.Now,
                                            StartSuperman = DateTime.Now
                                        };
                                        mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.SUPERMAN, "5", SmsTemp.SUPERMAN, chanel);
                                        break;
                                    case "VVG":
                                        newCustomer = new Customer()
                                        {
                                            Phone = User_ID,
                                            HHTD = 0,
                                            KTCD = 0,
                                            SUPERMAN = 0,
                                            VVG = 1,
                                            Createdate = DateTime.Now,
                                            StartVVG = DateTime.Now
                                        };
                                        mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.VVG_EN, "5", SmsTemp.VVG_VI, chanel);
                                        break;
                                }
                                db.Customers.Add(newCustomer);
                                db.SaveChanges();
                            }
                            else
                            {
                                // Kiểm tra thời gian tích lũy lần đầu, nếu quá 6 tháng thì reset
                                // DateTime? StartTimeActive = DateTime.Now;
                                DateTime? StartTimeActive = null;
                                switch (category)
                                {
                                    case "HHTD":
                                        // NumberOfCode = customer1.HHTD + 1;
                                        StartTimeActive = customer1.StartHHTD;
                                        break;
                                    case "KTCD":
                                        // NumberOfCode = customer1.KTCD + 1;
                                        StartTimeActive = customer1.StartKTCD;
                                        break;
                                    case "SUPERMAN":
                                        // NumberOfCode = customer1.SUPERMAN + 1;
                                        StartTimeActive = customer1.StartSuperman;
                                        break;
                                    case "VVG":
                                        // NumberOfCode = customer1.VVG + 1;
                                        StartTimeActive = customer1.StartVVG;
                                        break;
                                }

                                // Xử lý cho các thông tin thuê bao chưa có thời gian Active lần đầu.
                                if (StartTimeActive == null)
                                {
                                    log.Info(string.Format("Thuê bao {0} đang có Start Time Active là NULL", User_ID));
                                    var FirtTime = db.CodeGPs.Where(x => x.Phone == User_ID && x.Category == category && x.Status == 1).OrderBy(x => x.Activedate).FirstOrDefault();
                                    if (FirtTime != null && FirtTime.Activedate != null)
                                    {
                                        StartTimeActive = FirtTime.Activedate;
                                        switch (category)
                                        {
                                            case "HHTD":
                                                customer1.StartHHTD = FirtTime.Activedate;
                                                break;
                                            case "KTCD":
                                                customer1.StartKTCD = FirtTime.Activedate;
                                                break;
                                            case "SUPERMAN":
                                                customer1.StartSuperman = FirtTime.Activedate;
                                                break;
                                            case "VVG":
                                                customer1.StartVVG = FirtTime.Activedate;
                                                break;
                                        }
                                        log.Info(string.Format("Cập nhật thời gian kích hoạt nhóm sản phẩm {0} cho thuê bao {1} với thời gian là {2}", category, User_ID, StartTimeActive));
                                        db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                                // Tích lũy lần đầu
                                if (StartTimeActive == null)
                                {
                                    log.Info(string.Format("Thuê bao {0} kích hoạt sản phẩm lần đầu", User_ID));
                                    codeGP.Status = 1;
                                    codeGP.Phone = User_ID;
                                    codeGP.Activedate = DateTime.Now;
                                    // Cập nhật thông tin khách hàng
                                    db.Entry(codeGP).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    // Reset lại điểm của khách hàng
                                    switch (category)
                                    {
                                        case "HHTD":
                                            customer1.HHTD = 1;
                                            // customer1.StartHHTD = DateTime.Now;
                                            customer1.StartHHTD = codeGP.Activedate;
                                            mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.HHTD_EN, "5", SmsTemp.HHTD_VI, chanel);
                                            break;
                                        case "KTCD":
                                            customer1.KTCD = 1;
                                            // customer1.StartKTCD = DateTime.Now;
                                            customer1.StartKTCD = codeGP.Activedate;
                                            mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.KTCD_EN, "5", SmsTemp.KTCD_VI, chanel);
                                            break;
                                        case "SUPERMAN":
                                            customer1.SUPERMAN = 1;
                                            // customer1.StartSuperman = DateTime.Now;
                                            customer1.StartSuperman = codeGP.Activedate;
                                            mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.SUPERMAN, "5", SmsTemp.SUPERMAN, chanel);
                                            break;
                                        case "VVG":
                                            customer1.VVG = 1;
                                            // customer1.StartVVG = DateTime.Now;
                                            customer1.StartVVG = codeGP.Activedate;
                                            mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.VVG_EN, "5", SmsTemp.VVG_VI, chanel);
                                            break;
                                    }
                                    db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else if (StartTimeActive.GetValueOrDefault().AddMonths(6) >= DateTime.Now)
                                {
                                    DateTime CompareDate = StartTimeActive.GetValueOrDefault().AddMinutes(-1);
                                    log.Info(string.Format("Thuê bao {0} kích hoạt sản phẩm trong vòng 6 tháng", User_ID));
                                    // var Codes = db.CodeGPs.Where(x => x.Activedate >= StartTimeActive && x.Status == 1).Where(x => x.Category.Equals(category) && x.Phone == customer1.Phone);
                                    var Codes = db.CodeGPs.Where(x => x.Activedate >= CompareDate && x.Status == 1).Where(x => x.Category.Equals(category) && x.Phone == customer1.Phone);
                                    int NumberOfCode1 = Codes.Count();
                                    log.Info(string.Format("Thuê bao {0} kích hoạt sản phẩm {1} nhóm {2} có số lượng {3}", User_ID, Code, category, NumberOfCode1));
                                    if (NumberOfCode1 >= NUMBER_OVER_QUOTA)
                                    {
                                        mt_trakhachhang = SmsTemp.OVER_QUOTA(chanel);
                                        status = 5;
                                    }
                                    else
                                    {
                                        // Kiểm tra số lượng code đã tích lũy trong tháng 
                                        var Code_In_Month = db.CodeGPs.Where(x => x.Activedate.Value.Year == DateTime.Now.Year && x.Activedate.Value.Month == DateTime.Now.Month && x.Status == 1).Where(x => x.Category.Equals(category) && x.Phone == customer1.Phone);
                                        int NumberOfCode2 = Code_In_Month.Count();
                                        if (NumberOfCode2 >= NUMBER_OVER_QUOTA_IN_MONTH)
                                        {
                                            mt_trakhachhang = SmsTemp.OVER_QUOTA_IN_MONTH(chanel);
                                            status = 6;
                                        }
                                        else
                                        {
                                            // Tích lũy cho khách hàng.
                                            codeGP.Status = 1;
                                            codeGP.Phone = User_ID;
                                            codeGP.Activedate = DateTime.Now;
                                            // Cập nhật thông tin khách hàng
                                            db.Entry(codeGP).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges(); // Thêm ngày 31/07/2021
                                            string SmsVI = "";
                                            string SmsEN = "";
                                            int? NumberOfCode = 0;
                                            var customer2 = db.Customers.Where(a => a.Phone == User_ID).SingleOrDefault();
                                            log.Info(string.Format("Thuê bao {0} Có {1} HHTD, {2} KTCD, {3} Superman, {4} VVG", User_ID, customer2.HHTD, customer2.KTCD, customer2.SUPERMAN, customer2.VVG));
                                            switch (category)
                                            {
                                                case "HHTD":
                                                    customer2.HHTD++;
                                                    SmsVI = SmsTemp.HHTD_VI;
                                                    SmsEN = SmsTemp.HHTD_EN;
                                                    NumberOfCode = customer2.HHTD;
                                                    // customer1.HHTD = NumberOfCode;
                                                    break;
                                                case "KTCD":
                                                    customer2.KTCD++;
                                                    // customer1.KTCD = NumberOfCode;
                                                    SmsVI = SmsTemp.KTCD_VI;
                                                    SmsEN = SmsTemp.KTCD_EN;
                                                    NumberOfCode = customer2.KTCD;
                                                    break;
                                                case "SUPERMAN":
                                                    customer2.SUPERMAN++;
                                                    // customer1.SUPERMAN = NumberOfCode;
                                                    SmsVI = SmsTemp.SUPERMAN;
                                                    SmsEN = SmsTemp.SUPERMAN;
                                                    NumberOfCode = customer2.SUPERMAN;
                                                    break;
                                                case "VVG":
                                                    customer2.VVG++;
                                                    // customer1.VVG = NumberOfCode;
                                                    SmsVI = SmsTemp.VVG_VI;
                                                    SmsEN = SmsTemp.VVG_EN;
                                                    NumberOfCode = customer2.VVG;
                                                    break;
                                            }
                                            db.Entry(customer2).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

                                            if (NumberOfCode < 6)
                                            {
                                                int? SoHopConLai = 6 - NumberOfCode;
                                                mt_trakhachhang = SmsTemp.ACTIVE(NumberOfCode.ToString(), SmsEN, SoHopConLai.ToString(), SmsVI, chanel);
                                            }
                                            else if (NumberOfCode % 6 == 0)
                                            {
                                                var dem = db.GiftExchanges.Where(a => a.Phone == User_ID && a.Product == category);
                                                int demsolan = dem.Count();
                                                demsolan++;
                                                // Số hộp được tặng
                                                int? iHop = NumberOfCode / 6;
                                                // Insert vào bảng GiftExchange
                                                GiftExchange giftExchange = new GiftExchange()
                                                {
                                                    Phone = User_ID,
                                                    Product = category,
                                                    Createdate = DateTime.Now,
                                                    Status = 0,
                                                    Count = demsolan
                                                };
                                                db.GiftExchanges.Add(giftExchange);
                                                db.SaveChanges();
                                                mt_trakhachhang = SmsTemp.ACTIVE(NumberOfCode.ToString(), SmsEN, SmsVI, iHop, chanel);
                                            }
                                            else
                                            {
                                                int? iHop = NumberOfCode / 6;
                                                mt_trakhachhang = SmsTemp.ACTIVE(NumberOfCode.ToString(), SmsEN, SmsVI, iHop, chanel);
                                            }
                                        }
                                    }
                                }
                                else // Quá 6 tháng từ ngày kích hoạt sản phẩm đầu tiên
                                {
                                    // Kiểm tra số lượng code đã tích lũy trong tháng 
                                    var Code_In_Month = db.CodeGPs.Where(x => x.Activedate.Value.Year == DateTime.Now.Year && x.Activedate.Value.Month == DateTime.Now.Month && x.Status == 1).Where(x => x.Category.Equals(category) && x.Phone == customer1.Phone);
                                    int NumberOfCode2 = Code_In_Month.Count();
                                    if (NumberOfCode2 >= NUMBER_OVER_QUOTA_IN_MONTH)
                                    {
                                        mt_trakhachhang = SmsTemp.OVER_QUOTA_IN_MONTH(chanel);
                                        status = 6;
                                    }
                                    else
                                    {
                                        log.Info(string.Format("Thuê bao {0} quá 6 tháng kể từ ngày kích hoạt đầu tiên", User_ID));
                                        codeGP.Status = 1;
                                        codeGP.Phone = User_ID;
                                        codeGP.Activedate = DateTime.Now;
                                        // Cập nhật thông tin khách hàng
                                        db.Entry(codeGP).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        // Reset lại điểm của khách hàng
                                        switch (category)
                                        {
                                            case "HHTD":
                                                customer1.HHTD = 1;
                                                customer1.StartHHTD = DateTime.Now;
                                                mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.HHTD_EN, "5", SmsTemp.HHTD_VI, chanel);
                                                break;
                                            case "KTCD":
                                                customer1.KTCD = 1;
                                                customer1.StartKTCD = DateTime.Now;
                                                mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.KTCD_EN, "5", SmsTemp.KTCD_VI, chanel);
                                                break;
                                            case "SUPERMAN":
                                                customer1.SUPERMAN = 1;
                                                customer1.StartSuperman = DateTime.Now;
                                                mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.SUPERMAN, "5", SmsTemp.SUPERMAN, chanel);
                                                break;
                                            case "VVG":
                                                customer1.VVG = 1;
                                                customer1.StartVVG = DateTime.Now;
                                                mt_trakhachhang = SmsTemp.ACTIVE("1", SmsTemp.VVG_EN, "5", SmsTemp.VVG_VI, chanel);
                                                break;
                                        }
                                        db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                //// Cập nhật thông tin khách hàng
                                //string SmsVI = "";
                                //string SmsEN = "";
                                //switch (category)
                                //{
                                //    case "HHTD":
                                //        NumberOfCode = customer1.HHTD + 1;
                                //        SmsVI = SmsTemp.HHTD_VI;
                                //        SmsEN = SmsTemp.HHTD_EN;
                                //        customer1.HHTD = NumberOfCode;
                                //        break;
                                //    case "KTCD":
                                //        NumberOfCode = customer1.KTCD + 1;
                                //        customer1.KTCD = NumberOfCode;
                                //        SmsVI = SmsTemp.KTCD_VI;
                                //        SmsEN = SmsTemp.KTCD_EN;
                                //        break;
                                //    case "SUPERMAN":
                                //        NumberOfCode = customer1.SUPERMAN + 1;
                                //        customer1.SUPERMAN = NumberOfCode;
                                //        SmsVI = SmsTemp.SUPERMAN;
                                //        SmsEN = SmsTemp.SUPERMAN;
                                //        break;
                                //    case "VVG":
                                //        NumberOfCode = customer1.VVG + 1;
                                //        customer1.VVG = NumberOfCode;
                                //        SmsVI = SmsTemp.VVG_VI;
                                //        SmsEN = SmsTemp.VVG_EN;
                                //        break;
                                //}
                                //db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();

                                //if (NumberOfCode < 6)
                                //{
                                //    int? SoHopConLai = 6 - NumberOfCode;
                                //    mt_trakhachhang = SmsTemp.ACTIVE(NumberOfCode.ToString(), SmsEN, SoHopConLai.ToString(), SmsVI, chanel);
                                //}
                                //else if (NumberOfCode % 6 == 0)
                                //{
                                //    var dem = db.GiftExchanges.Where(a => a.Phone == User_ID && a.Product == category);
                                //    int demsolan = dem.Count();
                                //    demsolan++;
                                //    // Số hộp được tặng
                                //    int? iHop = NumberOfCode / 6;
                                //    // Insert vào bảng GiftExchange
                                //    GiftExchange giftExchange = new GiftExchange()
                                //    {
                                //        Phone = User_ID,
                                //        Product = category,
                                //        Createdate = DateTime.Now,
                                //        Status = 0,
                                //        Count = demsolan
                                //    };
                                //    db.GiftExchanges.Add(giftExchange);
                                //    db.SaveChanges();
                                //    mt_trakhachhang = SmsTemp.ACTIVE(NumberOfCode.ToString(), SmsEN, SmsVI, iHop, chanel);
                                //}
                                //else
                                //{
                                //    int? iHop = NumberOfCode / 6;
                                //    mt_trakhachhang = SmsTemp.ACTIVE(NumberOfCode.ToString(), SmsEN, SmsVI, iHop, chanel);
                                //}
                                //}
                                //else if ("KTCD".Equals(codeGP.Category))
                                //{
                                //    customer1.KTCD = NumberOfCode;
                                //    if (NumberOfCode < 6)
                                //    {
                                //        int? SoHopConLai = 6 - NumberOfCode;
                                //        mt_trakhachhang = SmsTemp.ACTIVE(customer1.KTCD.ToString(), SmsTemp.KTCD_EN, SoHopConLai.ToString(), SmsTemp.KTCD_VI, chanel);
                                //    }
                                //    else if (NumberOfCode % 6 == 0)
                                //    {
                                //        var dem = db.GiftExchanges.Where(a => a.Phone == User_ID && a.Product == codeGP.Category);
                                //        int demsolan = dem.Count();
                                //        demsolan++;
                                //        int? iHop = NumberOfCode / 6;
                                //        // Insert vào bảng GiftExchange
                                //        GiftExchange giftExchange = new GiftExchange()
                                //        {
                                //            Phone = User_ID,
                                //            Product = codeGP.Category,
                                //            Createdate = DateTime.Now,
                                //            Status = 0,
                                //            Count = demsolan
                                //        };
                                //        db.GiftExchanges.Add(giftExchange);
                                //        mt_trakhachhang = SmsTemp.ACTIVE(customer1.KTCD.ToString(), SmsTemp.KTCD_EN, SmsTemp.KTCD_VI, iHop, chanel);
                                //    }
                                //    else
                                //    {
                                //        int? iHop = NumberOfCode / 6;
                                //        mt_trakhachhang = SmsTemp.ACTIVE(customer1.KTCD.ToString(), SmsTemp.KTCD_EN, SmsTemp.KTCD_VI, iHop, chanel);
                                //    }
                                //}
                                //else if ("SUPERMAN".Equals(codeGP.Category))
                                //{
                                //    customer1.SUPERMAN = NumberOfCode;
                                //    db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    if (NumberOfCode < 6)
                                //    {
                                //        int? SoHopConLai = 6 - NumberOfCode;
                                //        mt_trakhachhang = SmsTemp.ACTIVE(customer1.SUPERMAN.ToString(), SmsTemp.SUPERMAN, SoHopConLai.ToString(), SmsTemp.SUPERMAN, chanel);
                                //    }
                                //    else if (NumberOfCode % 6 == 0)
                                //    {
                                //        var dem = db.GiftExchanges.Where(a => a.Phone == User_ID && a.Product == codeGP.Category);
                                //        int demsolan = dem.Count();
                                //        demsolan++;
                                //        int? iHop = NumberOfCode / 6;
                                //        // Insert vào bảng GiftExchange
                                //        GiftExchange giftExchange = new GiftExchange()
                                //        {
                                //            Phone = User_ID,
                                //            Product = codeGP.Category,
                                //            Createdate = DateTime.Now,
                                //            Status = 0,
                                //            Count = demsolan
                                //        };
                                //        db.GiftExchanges.Add(giftExchange);
                                //        db.SaveChanges();
                                //        mt_trakhachhang = SmsTemp.ACTIVE(customer1.SUPERMAN.ToString(), SmsTemp.SUPERMAN, SmsTemp.SUPERMAN, iHop, chanel);
                                //    }
                                //    else
                                //    {
                                //        int? iHop = NumberOfCode / 6;
                                //        mt_trakhachhang = SmsTemp.ACTIVE(customer1.SUPERMAN.ToString(), SmsTemp.SUPERMAN, SmsTemp.SUPERMAN, iHop, chanel);
                                //    }
                                //}
                                // db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                            }
                            // db.SaveChanges();
                            // }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
            var response = new HttpResponseMessage();
            // Lưu log xử lý vào bảng MT
            var LogMT = new LogMT()
            {
                MO_ID = id,
                Phone = User_ID,
                Createdate = DateTime.Now,
                Message = mt_trakhachhang,
                Product = category,
                Chanel = chanel,
                Status = status
            };
            db.LogMTs.Add(LogMT);
            db.SaveChanges();
            // Kết thúc lưu log MT
            var result = new Result()
            {
                status = "0",
                message = mt_trakhachhang,
                phone1 = User_ID
            };
            log.Info(string.Format("[MT] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, mt_trakhachhang));
            return ResponseMessage(result);
        }
        private HttpResponseMessage ResponseMessage(Result result)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
            return response;
        }

    }
}