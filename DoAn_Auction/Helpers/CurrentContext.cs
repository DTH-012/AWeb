using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_Auction.Helpers
{
    public class CurrentContext
    {
        public static bool IsLogged()
        {
            var flag = HttpContext.Current.Session["isLogin"];
            if (flag == null || (int)flag == 0)
            {
                //Kiểm tra thêm thông tin trong cookies
                //nếu có dùng thông tin trong cookies
                //để tái tạo lại session
                if(HttpContext.Current.Request.Cookies["userID"]!=null)
                {
                    int userIDCookie=Convert.ToInt32(HttpContext.Current.Request.Cookies["userID"].Value);
                    using (var ctx = new QLDauGiaEntities())
                    {
                        var model = ctx.Users
                            .Where(u => u.f_ID == userIDCookie)
                            .FirstOrDefault();
                        HttpContext.Current.Session["isLogin"]=1;
                        HttpContext.Current.Session["user"]=1;
                     }

                    return true;
                }
                
                return false;
            }
            return true;
        }
        public static User GetCurUser()
        {
            return (User)HttpContext.Current.Session["user"];
        }
        public static Wishlist GetWishlist()
        {
            var ret = (Wishlist)HttpContext.Current.Session["wishlist"];
            if (ret != null)
            {
                return ret;
            }
            ret = new Wishlist();
            HttpContext.Current.Session["wishlist"] = ret;
            return ret;
        }
        public static void Destroy()
        {
            HttpContext.Current.Session["isLogin"] = 0;
            HttpContext.Current.Session["user"] = null;

            HttpContext.Current.Response.Cookies["userID"].Expires = DateTime.Now.AddDays(-1);
        }
    }
}