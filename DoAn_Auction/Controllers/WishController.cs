using DoAn_Auction.Helpers;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class WishController : Controller
    {
        public ActionResult Add(int ProID)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var pro = ctx.Auctions.Where(p => p.ProID == ProID).FirstOrDefault();
                var item = new WishItem
                {
                    Pro = pro
                };
                CurrentContext.GetWishlist().AddItem(item);

            };
            return RedirectToAction("Detail", "Product");
        }

        //
        // GET: /Wish/
        public ActionResult Index()
        {
            return View();
        }

        
            
    }
}