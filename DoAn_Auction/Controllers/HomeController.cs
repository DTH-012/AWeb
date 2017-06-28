using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var dsSp = new List<Auction>();
                //Top5Hot
                var top5hot = new List<Auction>();
                var result = ctx.AuctionHistories
                    //.Where(a=>a.Status==true)
                    .GroupBy(p => p.ProID)
                    .OrderByDescending(a => a.Count())
                    .ToList();
                int count = 0;
                foreach (var p in result)
                {
                    int proid = (int)p.FirstOrDefault().ProID;
                    var pro = ctx.Auctions.Where(a => a.ProID == proid && a.Status == true).FirstOrDefault();
                    if (pro != null)
                    {
                        top5hot.Add(pro);
                        //dsSp.Add(pro);
                        count++;
                    }
                    if (count == 5)
                    {
                        break;
                    }
                }
                ViewBag.Top5Hot = top5hot.ToList();

                var top5highest= ctx.Auctions
                    .Where(p => p.Status == true)
                    .OrderByDescending(a => a.PriceCurrent)
                    .Take(5).ToList();
                ViewBag.Top5Highest = top5highest.ToList();

                var top5timeend = ctx.Auctions
                    .Where(p => p.Status == true)
                    .OrderBy(a => (a.TimeEnd))
                    .Take(5).ToList();
                ViewBag.Top5TimeEnd = top5timeend.ToList();

                var top5new = ctx.Auctions
                    .Where(p => p.Status == true)
                    .OrderByDescending(a => (a.TimeStart))
                    .Take(4).ToList();
                ViewBag.Top5New = top5new.ToList();

                dsSp.AddRange(top5hot);
                dsSp.AddRange(top5highest);
                dsSp.AddRange(top5timeend);
                dsSp.AddRange(top5new);
                return View(dsSp);
            }
        }

        public ActionResult Top5Hot()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var result = ctx.AuctionHistories
                    //.Where(a=>a.Status==true)
                    .GroupBy(p => p.ProID)
                    .OrderByDescending(a => a.Count())
                    .ToList();
                var proID = new List<int>();
                var dsSp = new List<Auction>();
                int count = 0;
                foreach (var p in result)
                {
                    int proid =(int) p.FirstOrDefault().ProID;
                    proID.Add(p.FirstOrDefault().ProID);
                    var pro = ctx.Auctions.Where(a => a.ProID == proid && a.Status==true).FirstOrDefault();
                    if (pro != null)
                    {
                        dsSp.Add(pro);
                        count++;
                    }
                    if(count==5)
                    {
                        break;
                    }
                }
                return PartialView("Top5HotPartial", dsSp);
            }
        }
        public ActionResult Top5Highest()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var dsSp = ctx.Auctions
                    .Where(p=>p.Status==true)
                    .OrderByDescending(a => a.PriceCurrent)
                    .Take(5).ToList();
                return PartialView("Top5HighestPartial", dsSp);
            }
        }
        public ActionResult Top5TimeEnd()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var dsSp = ctx.Auctions
                    .Where(p => p.Status == true)
                    .OrderBy(a => (a.TimeEnd))
                    .Take(5).ToList();
                return PartialView("Top5TimeEndPartial", dsSp);
            }
        }
        public ActionResult Top5New()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var dsSp = ctx.Auctions
                    .Where(p => p.Status == true)
                    .OrderByDescending(a => (a.TimeStart))
                    .Take(4).ToList();
                return PartialView("Top5NewPartial", dsSp);
            }
        } 
    }
}