using BotDetect.Web.Mvc;
using DoAn_Auction.Filters;
using DoAn_Auction.Helpers;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;

namespace DoAn_Auction.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

	//Re-up github
        // POST: Account/Register
        [HttpPost]
        [CaptchaValidation("CaptchaCode", "ExampleCaptcha", "Incorrect CAPTCHA code!")]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMsg = " Sai Captcha!";
            }
            else
            {
                ViewBag.Success = null;
                User u = new User
                {
                    f_Username = model.Username,
                    f_Password = StringUtils.MD5(model.RawPWD),
                    f_Name = model.Name,
                    f_Email = model.Email,
                    f_DOB = DateTime.ParseExact(model.DOB, "d/m/yyyy", null),
                    f_Address=model.Address,
                    f_Phone=model.Phone,
                    f_Level = 1,
                    //f_Money = 500000,
                    f_Rate = 0
                };

                using (QLDauGiaEntities ctx = new QLDauGiaEntities())
                {
                    ctx.Users.Add(u);
                    ctx.SaveChanges();
                    ViewBag.Success = "Đăng kí thành công";
                }
            }
            return View();
        }
         public bool IsUsernameUnique(string input)
        {
            bool check = true;
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Users
                    .Where(p => p.f_Username == input)
                    .FirstOrDefault();
                if (model != null)
                {
                    check = false;
                }
                return check;
            }
        }
        public bool IsEmailUnique(string input)
        {
            bool check = true;
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Users
                    .Where(p => p.f_Email == input)
                    .FirstOrDefault();
                if (model != null)
                {
                    check = false;
                }
                return check;
            }
        }

	// POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginVM model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                if(model.RawPWD==null || model.Username==null)
                {
                    TempData["ErrorMsg"] = "Đăng nhập không thành công";
                    return RedirectToAction("Register", "Account");
                }
                string encPwd = StringUtils.MD5(model.RawPWD);
                var user = ctx.Users
                    .Where(u => u.f_Username == model.Username && u.f_Password == encPwd)
                    .FirstOrDefault();
                if (user != null)
                {
                    Session["isLogin"] = 1;
                    Session["user"] = user;

                    if(model.Remember)
                    {
                        Response.Cookies["userID"].Value = user.f_ID.ToString();
                        Response.Cookies["userID"].Expires = DateTime.Now.AddDays(7);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMsg"] = "Đăng nhập không thành công";
                    return RedirectToAction("Register", "Account");
                }
            }
        }

        // POST: Account/Logout
        [HttpPost]
        public ActionResult Logout()
        {
            CurrentContext.Destroy();
            return RedirectToAction("Index", "Home");
        }
    }
}