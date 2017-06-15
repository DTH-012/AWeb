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
    public class MUserController : Controller
    {
        // GET: MUser/Index
        public ActionResult Index()
        {
            ViewBag.Current = "User";
            using (var ctx = new QLDauGiaEntities())
            {
                var users = ctx.Users
                    .OrderBy(u => u.f_ID)
                    .ToList();
                return View(users);
            }
        }
        // GET: MUser/Delete
        public ActionResult Delete(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("Index", "MUser");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                var user = ctx.Users.Where(c => c.f_ID == id)
                    .FirstOrDefault();
                return View(user);
            }
        }

        // POST: MUser/Delete
        [HttpPost]
        public ActionResult Delete(int idDelete)
        {

            using (var ctx = new QLDauGiaEntities())
            {
                var userdel = ctx.Users.Where(c => c.f_ID == idDelete)
                    .FirstOrDefault();
                userdel.f_Level = 0;
                ctx.Entry(userdel).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
                return RedirectToAction("Index", "MUser");
            }
        }
    }
}