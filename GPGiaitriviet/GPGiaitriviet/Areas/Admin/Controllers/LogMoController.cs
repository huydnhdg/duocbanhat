using GPGiaitriviet.Areas.Admin.Data;
using GPGiaitriviet.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class LogMoController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        static string url;
        public ActionResult Index(string searchstring, int? page, string from_date, string to_date, string status,
            string User_ID, string product, string chanel)
        {
            var model = from a in db.LogMTs
                        join b in db.LogMOes on a.MO_ID equals b.ID
                        select new LogMoMt()
                        {
                            ID = a.ID,
                            Phone = a.Phone,
                            Status = a.Status,
                            Createdate = a.Createdate,
                            Chanel = a.Chanel,
                            Message = a.Message,
                            MO_ID = a.MO_ID,
                            Product = a.Product,
                            Mo = b.Message
                        };
            // khi xem chi tiet khach hang
            if (!string.IsNullOrEmpty(User_ID))
            {
                model = model.Where(a => a.Phone == User_ID);
                var customer = db.Customers.SingleOrDefault(a => a.Phone == User_ID);
                ViewBag.user = customer;

                var pointofcustomer = new PointofCustomer()
                {
                    // Số điểm hiện tại
                    HHTD = customer.HHTD ?? 0,
                    KTCD = customer.KTCD ?? 0,
                    SUPERMAN = customer.SUPERMAN ?? 0,
                    VVG = customer.VVG ?? 0,

                    // Số điểm tích lũy, đang đổi.
                    HHTD_tong = db.CodeGPs.Where(a => a.Phone == customer.Phone && a.Category == "HHTD").Count(),
                    HHTD_doi = db.GiftExchanges.Where(a => a.Phone == customer.Phone && a.Product == "HHTD").Count(),

                    KTCD_tong = db.CodeGPs.Where(a => a.Phone == customer.Phone && a.Category == "KTCD").Count(),
                    KTCD_doi = db.GiftExchanges.Where(a => a.Phone == customer.Phone && a.Product == "KTCD").Count(),

                    SUPERMAN_tong = db.CodeGPs.Where(a => a.Phone == customer.Phone && a.Category == "SUPERMAN").Count(),
                    SUPERMAN_doi = db.GiftExchanges.Where(a => a.Phone == customer.Phone && a.Product == "SUPERMAN").Count(),

                    VVG_tong = db.CodeGPs.Where(a => a.Phone == customer.Phone && a.Category == "VVG").Count(),
                    VVG_doi = db.GiftExchanges.Where(a => a.Phone == customer.Phone && a.Product == "VVG").Count()
                };
                ViewBag.point = pointofcustomer;
                ViewBag.User_ID = User_ID;
            }
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a => a.Phone.Contains(searchstring) || a.Mo.Contains(searchstring));
                ViewBag.searchstring = searchstring;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(s => s.Createdate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                d = d.AddDays(1);
                model = model.Where(s => s.Createdate <= d);
                ViewBag.to_date = to_date;
            }
            if (!string.IsNullOrEmpty(status))
            {
                model = model.Where(a => a.Status.ToString() == status);
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(product))
            {
                model = model.Where(a => a.Product.ToString() == product);
                ViewBag.product = product;
            }
            if (!string.IsNullOrEmpty(chanel))
            {
                model = model.Where(a => a.Chanel.ToString() == chanel);
                ViewBag.chanel = chanel;
            }

            url = Request.Url.AbsoluteUri;

            IEnumerable<LogMoMt> result = model as IEnumerable<LogMoMt>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(result.OrderByDescending(a => a.Createdate).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult EditCustomer(string phone)
        {
            var tele = from a in db.AspNetUsers
                       from b in a.AspNetRoles
                       where b.Id == "Telesale"
                       select a;
            TempData["tele"] = tele.ToList();
            // TempData["cate"] = db.AspNetUsers.ToList();
            var customer1 = db.Customers.SingleOrDefault(a => a.Phone == phone);
            return PartialView("~/Areas/Admin/Views/LogMo/_EditCustomer.cshtml", customer1);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomers([Bind(Include = "")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.Editdate = DateTime.Now;
                // Set năm sinh
                if (customer.Birthday != null)
                {
                    if (customer.Birthday.Length >= 4)
                    {
                        customer.YearOfBorn = int.Parse(customer.Birthday.Substring(customer.Birthday.Length - 4));
                    }
                }
                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Redirect(url);
            }
            return Redirect(url);

        }
    }
}