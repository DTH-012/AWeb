using DoAn_Auction.Helpers;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product/Index
        public ActionResult Index(int page = 1)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                int n = ctx.Auctions.Count();

                int RecordPerPage = 12;

                int nPages = n / RecordPerPage;

                int m = n % RecordPerPage;
                if (m > 0)
                {
                    nPages++;
                }

                ViewBag.Pages = nPages;

                ViewBag.CurPage = page;

                var list = ctx.Auctions
                    .OrderBy(p => p.ProID)
                    .Skip((page - 1) * RecordPerPage)
                    .Take(RecordPerPage)
                    .ToList();
                return View(list);

            }
        }

        // GET: Product/Detail
        public ActionResult Detail(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("Index", "Home");
            }


            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Auctions
                    .Where(p => p.ProID == id)
                    .FirstOrDefault();
                var result = from u in ctx.Users
                             join ah in ctx.AuctionHistories on u.f_ID equals ah.UserID
                             where ah.ProID==id && ah.Status==true
                             //into g
                             select new AuctionHistoryVM
                             {
                                 ProID=ah.ProID,UserID=ah.UserID,Time=ah.Time,UserName=u.f_Name,Price=ah.Price,
                             };
                ViewBag.AuHis = result.ToList();
                return View(model);
            }
        }

    }
}