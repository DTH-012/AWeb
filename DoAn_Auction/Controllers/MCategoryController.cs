using DoAn_Auction.Filters;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    [CheckLogin(RequiredPermission=3)]
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
                //if (model.CatID == 0)
                //{
                //    var temp = true;
                //    //var cate = new Category
                //    //{
                //    //    CatName = model.CatName,
                //    //    ParentID = 0,
                //    //};
                //    model.ParentID = 0;
                //    ctx.Categories.Add(model);
                //    ctx.SaveChanges();
                //    return RedirectToAction("Index", "MCategory");
                //}
                ctx.Categories.Add(model);
                ctx.SaveChanges();
                //var cat = ctx.Categories.Where(c => c.ParentID != 0)
                //    .ToList();
                return RedirectToAction("Index", "MCategory");
            }
        }

        // GET: Products/Delete
        public ActionResult Delete(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("Index", "MCategory");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                var cat = ctx.Categories.Where(c => c.CatID == id)
                    .FirstOrDefault();
                return View(cat);
            }
        }

        // POST: Products/Delete
        [HttpPost]
        public ActionResult Delete(int idDelete)
        {

            using (var ctx = new QLDauGiaEntities())
            {
                var catdel = ctx.Categories.Where(c => c.CatID == idDelete)
                    .FirstOrDefault();
                ctx.Categories.Remove(catdel);
                ctx.SaveChanges();
                return RedirectToAction("Index", "MCategory");
            }
        }

        // GET: Products/Edit
        public ActionResult Edit(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("Index", "MCategory");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                var cat = ctx.Categories.Where(c => c.CatID == id)
                    .FirstOrDefault();
                return View(cat);
            }
        }

        // POST: Products/Edit
        [HttpPost]
        public ActionResult Edit(Category model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
                return RedirectToAction("Index", "MCategory");
            }
        }
    }
}