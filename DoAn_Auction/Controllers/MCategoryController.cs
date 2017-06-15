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
    public class MCategoryController : Controller
    {
        // GET: MCategory
        public ActionResult Index()
        {
            ViewBag.Current = "Category";
            using (var ctx = new QLDauGiaEntities())
            {
                var cat = ctx.Categories.Where(c => c.ParentID != 0)
                    .ToList();
                return View(cat);
            }
        }


        // GET: MCategory/Add
        public ActionResult Add()
        {
            ViewBag.Current = "Category";
            using (var ctx = new QLDauGiaEntities())
            {
                var cat = ctx.Categories.Where(c => c.ParentID == 0)
                    .ToList();
                return View(cat);
            }
        }

        // POST: MCategory/Add
        [HttpPost]
        public ActionResult Add(Category model)
        {
            ViewBag.Current = "Category";
            using (var ctx = new QLDauGiaEntities())
            {
                ctx.Categories.Add(model);
                ctx.SaveChanges();
                var cat = ctx.Categories.Where(c => c.ParentID != 0)
                    .ToList();
                return RedirectToAction("Index", "MCategory");
            }
        }
    }
}
