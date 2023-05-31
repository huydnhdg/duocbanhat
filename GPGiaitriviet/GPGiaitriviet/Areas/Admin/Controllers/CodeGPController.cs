using GPGiaitriviet.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class CodeGPController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        public ActionResult Index(string searchstring, int? page, string from_date, string to_date, string status,
            string product)
        {
            var model = from a in db.CodeGPs
                       select a;
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a => a.Code.Contains(searchstring) || a.Serial.Contains(searchstring));
                ViewBag.searchstring = searchstring;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                // ViewBag.from_date = from_date;
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(s => s.Activedate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                d = d.AddDays(1);
                model = model.Where(s => s.Activedate <= d);
                ViewBag.to_date = to_date;
            }
            if (!string.IsNullOrEmpty(status))
            {
                if ("1".Equals(status))
                {
                    model = model.Where(a => a.Status.ToString() == status);
                } else
                {
                    model = model.Where(a => a.Status == null);
                }
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(product))
            {
                model = model.Where(a => a.Category.ToString() == product);
                ViewBag.product = product;
            }
            IEnumerable<CodeGP> result = model as IEnumerable<CodeGP>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(result.OrderBy(a => a.Code).ToPagedList(pageNumber, pageSize));
        }
        // GET: Admin/Article/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CodeGP code = db.CodeGPs.SingleOrDefault(a => a.ID == id);
            if (code == null)
            {
                return HttpNotFound();
            }
            return View(code);
        }

        // POST: Admin/Article/Edit/5
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] CodeGP code)
        {
            try
            {
                // TODO: Add update logic here
                db.Entry(code).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(code);
            }
        }

        // GET: Admin/Article/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CodeGP code = db.CodeGPs.SingleOrDefault(a => a.ID == id);
            if (code == null)
            {
                return HttpNotFound();
            }
            return View(code);
        }

        // POST: Admin/Article/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long? id)
        {
            CodeGP code = db.CodeGPs.Find(id);
            try
            {
                db.CodeGPs.Remove(code);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View(code);
            }
        }
    }
}