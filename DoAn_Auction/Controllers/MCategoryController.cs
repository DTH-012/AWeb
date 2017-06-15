using DoAn_Auction.Filters;
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
            return View();
        }


        // GET: MCategory/Add
        public ActionResult Add()
        {
            return View();
        }
    }
}