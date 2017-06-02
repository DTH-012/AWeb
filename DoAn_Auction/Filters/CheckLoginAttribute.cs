﻿using DoAn_Auction.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Filters
{
    public class CheckLoginAttribute:ActionFilterAttribute
    {
        public int RequiredPermission { get; set; }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (CurrentContext.IsLogged() == false)
            {
                filterContext.Result = new RedirectResult("~/Account/Register");
            }

            if (CurrentContext.GetCurUser().f_Level < 2)
            {
                filterContext.Result = new RedirectResult("~/Account/Register");
                return;
            }

            if(CurrentContext.GetCurUser().f_Level<3)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            base.OnActionExecuted(filterContext);
        }

    }
}