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
            return View();
        }

        QLDauGiaEntities ctx = new QLDauGiaEntities();
        public ActionResult LoadTOPBID()
        {
            var list = ctx.Auctions.OrderByDescending(p => p.Customer).Take(4).ToList();
            return PartialView("TOPBIDPartial", list);
        }
		public ActionResult Top5Hot()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var result = ctx.AuctionHistories
                    .Where(a=>a.Status==true)
                    .GroupBy(p => p.ProID)
                    .OrderByDescending(a => a.Count())
                    .Take(5);
                var proID = new List<int>();
                var dsSp = new List<Auction>();
                foreach (var p in result)
                {
                    int proid =(int) p.FirstOrDefault().ProID;
                    proID.Add(p.FirstOrDefault().ProID);
                    var pro = ctx.Auctions.Where(a => a.ProID == proid).FirstOrDefault();
                    dsSp.Add(pro);
                }
                return PartialView("Top5HotPartial", dsSp);
            }
        }
    }
}