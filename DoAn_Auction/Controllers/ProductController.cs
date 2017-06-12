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
		//Commit loi - Reup
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
            if (CurrentContext.IsLogged() == false)
            {
                return View();
            }
            int UID = (int)CurrentContext.GetCurUser().f_ID;
            Decimal uPriceSet = Convert.ToDecimal(PriceSet);
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Auctions
                    .Where(p => p.ProID == ProID)
                    .FirstOrDefault();

                var result = from u in ctx.Users
                             join ah in ctx.AuctionHistories on u.f_ID equals ah.UserID
                             where ah.ProID == ProID && ah.Status==true
                             //into g
                             select new AuctionHistoryVM
                             {
                                 ProID=ah.ProID,
                                 UserID=ah.UserID,
                                 Time = ah.Time,
                                 UserName = u.f_Name,
                                 Price = ah.Price,
                             };
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
                        UserID = UID,
                        Price = model.PriceCurrent,
                        Time=DateTime.Now,
                        Status=true,
                    };
                    ctx.AuctionHistories.Add(auHis);
                    ctx.SaveChanges();
                    ViewBag.AuHis = result.ToList();
                    return View(model);
                }
                else
                {
                    if (uPriceSet == model.PriceHighest)
                    {
                        model.PriceCurrent = uPriceSet;
                        ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        ViewBag.ErrorMsg = "Đã có người đặt giá trước bạn bạn ! Vui lòng đặt giá khác";
                        var auHis = new AuctionHistory
                        {
                            ProID = model.ProID,
                            UserID = UID,
                            Price = model.PriceCurrent,
                            Time = DateTime.Now,
                            Status = true,
                        };
                        ctx.AuctionHistories.Add(auHis);
                        ctx.SaveChanges();
                        ViewBag.AuHis = result.ToList();
                        return View(model);

                    }
                    else
                    {
                        if (uPriceSet > model.PriceCurrent)
                        {
                            model.PriceCurrent = uPriceSet + model.Step;
                            ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                            ctx.SaveChanges();
                            ViewBag.ErrorMsg = "Đã có người đặt giá cao hơn bạn ! Vui lòng đặt giá khác";
                            var auHis = new AuctionHistory
                            {
                                ProID = model.ProID,
                                UserID = UID,
                                Price = model.PriceCurrent,
                                Time = DateTime.Now,
                                Status = true,
                            };
                            ctx.AuctionHistories.Add(auHis);
                            ctx.SaveChanges();
                            ViewBag.AuHis = result.ToList();
                            return View(model);
                        }
                    }
                }

                ViewBag.AuHis = result.ToList();
                return View(model);
            }
        } 

        // GET: Product/Detail
        public ActionResult KickUser(int? ProID, int? KUserID)
        {
            if (ProID.HasValue == false || KUserID.HasValue == false)
            {
                return RedirectToAction("Index", "Product");
            }
            using (var ctx = new QLDauGiaEntities())
            {
                var auhis = ctx.AuctionHistories
                    .Where(p => p.ProID == ProID && p.UserID == KUserID)
                    .ToList();
                foreach (var p in auhis)
                {
                    p.Status = false;
                    ctx.Entry(p).State = System.Data.Entity.EntityState.Modified;
                }
                ctx.SaveChanges();
                var au = ctx.AuctionHistories.OrderByDescending(p => p.Price)
                    .Where(p => p.ProID == ProID && p.Status==true).Take(1).FirstOrDefault();
                var pro = ctx.Auctions.Where(p => p.ProID == ProID).FirstOrDefault();
                if(pro.Customer==KUserID)
                {
                    pro.PriceHighest = au.Price;
                    pro.PriceCurrent = au.Price;
                    pro.Customer = au.UserID;
                    ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("Detail", "Product", new { id = ProID });
        }
    }
}