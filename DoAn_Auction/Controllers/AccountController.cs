﻿using BotDetect.Web.Mvc;
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
                    f_Like = 0,
                    f_Dislike=0,
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
        //public JsonResult IsUsernameUnique(string input)
        //{
        //    //TODO: Do the validation
        //    JsonResult result = new JsonResult();
        //    var ctx = new QLDauGiaEntities();
        //        var model = ctx.Users
        //            .Where(p => p.f_Username == input)
        //            .FirstOrDefault();
        //        if (model != null)
        //        {
        //            result.Data = true;
        //        }
        //        result.Data = false;
        //        return result;
        //}
        //public JsonResult IsEmailUnique(string input)
        //{
        //    //TODO: Do the validation
        //    JsonResult result = new JsonResult();
        //    result.Data = false;
        //    using (var ctx = new QLDauGiaEntities())
        //    {
        //        var model = ctx.Users
        //            .Where(p => p.f_Email == input)
        //            .FirstOrDefault();
        //        if (model != null)
        //        {
        //            result.Data = true;
        //        }
        //        return result;
        //    }
        //}

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
                    .Where(u => u.f_Username == model.Username && u.f_Password == encPwd && u.f_Level>0)
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

        // GET: Account/Profile
        [CheckLogin]
        public ActionResult Profile()
        {
            var ctx = new QLDauGiaEntities();
            int ID =(int) CurrentContext.GetCurUser().f_ID;
            var user = ctx.Users.Where(p => p.f_ID == ID).FirstOrDefault();
            return View(user);
        }

        // GET: Account/EditProfile
        [CheckLogin]
        public ActionResult EditProfile()
        {
            return View();
        }
        // POST: Account/EditProfile
        [HttpPost]
        public ActionResult EditProfile(RegisterVM model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var user = ctx.Users.Where(p => p.f_Username == model.Username).FirstOrDefault();
                if(user.f_Password==StringUtils.MD5(model.RawPWD))
                {
                    user.f_Name = model.Name;
                    user.f_Email = model.Email;
                    user.f_DOB = DateTime.ParseExact(model.DOB, "d/m/yyyy", null);
                    user.f_Address = model.Address;
                    user.f_Phone = model.Phone;
                    ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    return RedirectToAction("Profile", "Account");
                }
                return View();
            }
        }

        // GET: Account/ChangePassword
        [CheckLogin]
        public ActionResult ChangePassword()
        {
            return View();
        }
        // POST: Account/ChangePassword
        [HttpPost]
        public ActionResult ChangePassword(string RawPWD,string NewRawPWD)
        {
            if (CurrentContext.IsLogged() == false)
            {
                return View();
            }
            int ID = (int)CurrentContext.GetCurUser().f_ID;
            using (var ctx = new QLDauGiaEntities())
            {
                var user = ctx.Users.Where(p => p.f_ID ==ID).FirstOrDefault();
                if (user.f_Password == StringUtils.MD5(RawPWD))
                {
                    user.f_Password = StringUtils.MD5(NewRawPWD);
                    ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    CurrentContext.Destroy();
                    return RedirectToAction("Index", "Home");
                }
                return View();
            }
        }

        // GET: Account/ReviewDetail
        [CheckLogin]
        public ActionResult ReviewDetail()
        {
            if (CurrentContext.IsLogged() == false)
            {
                return View();
            }
            int UserID = (int)CurrentContext.GetCurUser().f_ID;
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Reviews
                    .Where(r => r.Receiver == UserID).ToList();
                return View(model);
            }
        }


        // POST: Account/Favorite
        [HttpPost]
        public ActionResult AddFavo(int ProID)
        {
            if (CurrentContext.IsLogged() == false)
            {
                return View();
            }
            int UserID = (int)CurrentContext.GetCurUser().f_ID;
            using (var ctx = new QLDauGiaEntities())
            {
                if(ctx.Favorites.Where(f=>f.UserID==UserID).Any(f=>f.ProID==ProID))
                {
                    return Json(null,JsonRequestBehavior.AllowGet);
                }
                var model = new Favorite
                {
                    UserID = UserID,
                    ProID = ProID,
                };
                ctx.Favorites.Add(model);
                ctx.SaveChanges();
                return Json(JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Account/Favorite
        [CheckLogin]
        public ActionResult Favorite()
        {
            if (CurrentContext.IsLogged() == false)
            {
                return View();
            }
            int UserID = (int)CurrentContext.GetCurUser().f_ID;
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Favorites.Where(f => f.UserID == UserID).ToList();
                var list= new List<Auction>();
                foreach(var f in model)
                {
                    var pro = ctx.Auctions
                        .Where(p => p.ProID == f.ProID).FirstOrDefault();
                    if (pro != null)
                    {
                        list.Add(pro);
                    }
                }
                return View(list);
            }
        }

        // POST: Account/RemoveFavorite
        [HttpPost]
        public ActionResult RemoveFavorite(int ProID)
        {
            if (CurrentContext.IsLogged() == false)
            {
                return Json(null,JsonRequestBehavior.AllowGet);
            }
            int UserID = (int)CurrentContext.GetCurUser().f_ID;
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Favorites.Where(f => f.ProID == ProID && f.UserID == UserID)
                    .FirstOrDefault();
                ctx.Favorites.Remove(model);
                ctx.SaveChanges();
                return Json(JsonRequestBehavior.AllowGet);
            }
        }
    }
}