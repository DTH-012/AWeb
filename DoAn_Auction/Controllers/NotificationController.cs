using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult Index(int? Input=2)
        {
            if(Input.HasValue==false)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Input = Input;
            return View();
        }
    }
}