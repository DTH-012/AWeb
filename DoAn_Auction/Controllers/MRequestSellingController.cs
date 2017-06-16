using DoAn_Auction.Filters;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    [CheckLogin(RequiredPermission = 3)]
    public class MRequestSellingController : Controller
    {
        // GET: MRequestSelling/Index
        public ActionResult Index()
        {
            ViewBag.Current = "Request";
            using (var ctx = new QLDauGiaEntities())
            {
                var cat = ctx.RegisterSellings.Where(r=>r.Status==1)
                    .OrderBy(r=>r.DateSend)
                    .ToList();
                return View(cat);
            }
        }

        //POST: MRequestSelling/Accept
        [HttpPost]
        public ActionResult Accept(int id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var rq = ctx.RegisterSellings.Where(r => r.ID == id)
                    .FirstOrDefault();
                var u = ctx.Users.Where(us => us.f_ID == rq.UserID).FirstOrDefault();
                u.f_Level = 2;

                rq.DateStart = DateTime.Now;
                rq.DateEnd = DateTime.Now.AddDays(7);
                rq.Status = 2;

                ctx.Entry(rq).State = System.Data.Entity.EntityState.Modified;
                ctx.Entry(u).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
                return Json(JsonRequestBehavior.AllowGet);
            }
        }
    }
}