using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        QLDauGiaEntities ctx = new QLDauGiaEntities();
        public ActionResult Form()
        {
            var list = ctx.Categories
                    .Where(c => c.ParentID == 0).ToList();
            return PartialView("FormPartial", list);
        }

       
	//Function Seach 
        public ActionResult Result(string q, int? cat,int? sort, int page = 1)
        {
            var list=ctx.Auctions.ToList();
            //int n = 0;
            if (cat.HasValue == false || cat==0)
            {
                list= ctx.Auctions
                   .Where(c => c.ProName.Contains(q)).ToList();
            }
            else
            {
                list = ctx.Auctions
                    .Where(c => c.ProName.Contains(q) && c.CatID==cat).ToList();
            }

            int n = list.Count();
            int RecordPerPage = 8;

            int nPages = n / RecordPerPage;

            int m = n % RecordPerPage;
            if (m > 0)
            {
                nPages++;
            }

            ViewBag.Pages = nPages;

            ViewBag.CurPage = page;
            if (sort.HasValue == true)
            {
                if (sort == 0)
                {
                    list = list.OrderByDescending(p => p.TimeEnd).ToList();
                }
                else
                {
                    list = list.OrderBy(p => p.PriceCurrent).ToList();
                }
                ViewBag.sort = sort;
            }
            else
            {
                list = list.OrderBy(p => p.ProID).ToList();
            }
            list = list.Skip((page - 1) * RecordPerPage)
                    .Take(RecordPerPage)
                    .ToList();
            //TempData["q"] = q;
            //TempData["cat"] = cat;
            ViewBag.q = q;
            ViewBag.cat = cat;
            return View(list);
        }
        public ActionResult sort(string q, int? cat,int? sort, int page = 1)
        {
            var list = ctx.Auctions.ToList();
            //int n = 0;
            if (cat.HasValue == false || cat == 0)
            {
                list = ctx.Auctions
                   .Where(c => c.ProName.Contains(q)).ToList();
            }
            else
            {
                list = ctx.Auctions
                    .Where(c => c.ProName.Contains(q) && c.CatID == cat).ToList();
            }

            int n = list.Count();
            int RecordPerPage = 8;

            int nPages = n / RecordPerPage;

            int m = n % RecordPerPage;
            if (m > 0)
            {
                nPages++;
            }

            ViewBag.Pages = nPages;

            ViewBag.CurPage = page;
            if(sort.HasValue==true)
            {
                if(sort==0)
                {
                    list = list.OrderByDescending(p => p.TimeEnd).ToList();
                }
                else
                {
                    list = list.OrderBy(p => p.PriceCurrent).ToList();
                }
            }
            list = list.Skip((page - 1) * RecordPerPage)
                    .Take(RecordPerPage)
                    .ToList();
            //TempData["q"] = q;
            //TempData["cat"] = cat;
            ViewBag.q = q;
            ViewBag.cat = cat;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}