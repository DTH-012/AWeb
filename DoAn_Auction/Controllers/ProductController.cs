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

                //ViewBag.Pros = from a in ctx.Auctions
                //             join o in ctx.AuctionHistories on a.ProID equals o.ProID
                //             group a by new {a.ProID,a.ProName,a.PriceHighest,a.TimeStart }into g
                //             select new
                //             {
                //                 Count = g.Count()
                //             };
                return View(list);

            }
        }

        // GET: Product/Detail
        public ActionResult Detail(int? id,int page=1)
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
                             where ah.ProID==id
                             //into g
                             select new AuctionHistoryVM
                             {
                                 Time=ah.Time,UserName=u.f_Name,Price=ah.Price,
                             };
                int n = result.Count();

                int RecordPerPage = 8;

                int nPages = n / RecordPerPage;

                int m = n % RecordPerPage;
                if (m > 0)
                {
                    nPages++;
                }

                ViewBag.Pages = nPages;

                ViewBag.CurPage = page;

                //var list = ctx.Auctions
                //    .OrderBy(p => p.ProID)
                //    .Skip((page - 1) * RecordPerPage)
                //    .Take(RecordPerPage)
                //    .ToList();
                ViewBag.AuHis = result.Skip((page - 1) * RecordPerPage)
                    .Take(RecordPerPage).ToList();
                return View(model);
            }
        }

        // POST: Product/Detail - Đấu giá
        [HttpPost]
        public ActionResult Detail(int ProID, string PriceSet)
        {
            Decimal uPriceSet = Convert.ToDecimal(PriceSet);
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Auctions
                    .Where(p => p.ProID == ProID)
                    .FirstOrDefault();
                if (uPriceSet > model.PriceHighest)
                {
                    model.PriceCurrent = model.PriceHighest + model.Step;
                    model.PriceHighest = uPriceSet;
                    ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    ViewBag.Success = "Đặt giá thành công ! Bạn đang giữ giá cao nhất";
                    var auHis = new AuctionHistory
                    {
                        ProID = model.ProID,
                        UserID = 1,
                        Price = model.PriceCurrent - +model.Step,
                        Time=DateTime.Now,
                    };
                    ctx.AuctionHistories.Add(auHis);
                    ctx.SaveChanges();
                    return View(model);
                }
                else
                {
                    if(uPriceSet > model.PriceCurrent)
                    {
                        model.PriceCurrent = uPriceSet + model.Step;
                        ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        ViewBag.ErrorMsg = "Đã có người đặt giá cao hơn bạn ! Vui lòng đặt giá khác";
                        var auHis = new AuctionHistory
                        {
                            ProID = model.ProID,
                            UserID = 3,
                            Price = model.PriceCurrent - +model.Step,
                            Time = DateTime.Now,
                        };
                        ctx.AuctionHistories.Add(auHis);
                        ctx.SaveChanges();
                        return View(model);
                    }
                }

                return View(model);
            }
        } 
    }
}