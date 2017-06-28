using DoAn_Auction.Filters;
using DoAn_Auction.Helpers;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAn_Auction
{
    public class MvcApplication : System.Web.HttpApplication
    {
        //protected void Application_Start()
        //{
        //    AreaRegistration.RegisterAllAreas();
        //    RouteConfig.RegisterRoutes(RouteTable.Routes);
        //}
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Create a new Timer with Interval set to 60 seconds(1 Minutes).
            System.Timers.Timer aTimer = new System.Timers.Timer(1 * 60 * 1000);
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            aTimer.Start();
        }
        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var proList = ctx.Auctions.Where(p => p.TimeEnd <= DateTime.Now && p.Status==true).ToList();
                var requestList = ctx.RegisterSellings.Where(r => r.DateEnd<=DateTime.Now && r.Status == 2).ToList();
                foreach (var r in requestList)
                {
                    r.Status = 0;
                    ctx.Entry(r).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
                foreach (var pro in proList)
                {
                    //Update status
                    pro.Status = false;
                    ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    var seller = ctx.Users.Where(u => u.f_ID == pro.Seller).FirstOrDefault();
                    string contentSN = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/assets/mailMsg/tplAuction.html"));
                    string contentS = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/assets/mailMsg/tplAuction.html"));
                    string contentW = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/assets/mailMsg/tplAuction.html"));
                    //Send Email
                    if (pro.Customer == 0)
                    {
                        //Email to Seller
                        contentSN = contentSN.Replace("{{ColorTitle}}", "#ffad33");
                        contentSN = contentSN.Replace("{{Title}}", "Sản phẩm đã đăng");
                        contentSN = contentSN.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                        contentSN = contentSN.Replace("{{UName}}", seller.f_Name);
                        contentSN = contentSN.Replace("{{Msg}}", "Đấu giá kết thúc, không có ai đấu giá sản phẩm của bạn");
                        contentSN = contentSN.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                        contentSN = contentSN.Replace("{{ProName}}", pro.ProName);
                        contentSN = contentSN.Replace("{{ProLink}}", HostingEnvironment.MapPath("~/Product/Detail/"+pro.ProID.ToString()));
                        contentSN = contentSN.Replace("{{Home}}", HostingEnvironment.MapPath("~/Home"));
                        MailHelper.SendEmail(seller.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentSN);
                    }
                    else
                    {
                        var winner = ctx.Users.Where(u => u.f_ID == pro.Customer).FirstOrDefault();
                        //Email to Seller
                        contentS = contentS.Replace("{{ColorTitle}}", "#66ff33");
                        contentS = contentS.Replace("{{Title}}", "Sản phẩm đã đăng");
                        contentS = contentS.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                        contentS = contentS.Replace("{{UName}}", seller.f_Name);
                        contentS = contentS.Replace("{{Msg}}", "Đấu giá kết thúc, đã có người chiến thắng sản phẩm của bạn");
                        contentS = contentS.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                        contentS = contentS.Replace("{{ProName}}", pro.ProName);
                        contentS = contentS.Replace("{{ProLink}}", HostingEnvironment.MapPath("~/Product/Detail/" + pro.ProID.ToString()));
                        contentS = contentS.Replace("{{Home}}", HostingEnvironment.MapPath("~/Home"));
                        MailHelper.SendEmail(seller.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentS);
                        //Email to Winner
                        contentW = contentW.Replace("{{ColorTitle}}", "#66ff33");
                        contentW = contentW.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                        contentW = contentW.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                        contentW = contentW.Replace("{{UName}}", winner.f_Name);
                        contentW = contentW.Replace("{{Msg}}", "Đấu giá kết thúc, bạn đã chiến thắng trong một sản phẩm");
                        contentW = contentW.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                        contentW = contentW.Replace("{{ProName}}", pro.ProName);
                        contentW = contentW.Replace("{{ProLink}}", HostingEnvironment.MapPath("~/Product/Detail/" + pro.ProID.ToString()));
                        contentW = contentW.Replace("{{Home}}", HostingEnvironment.MapPath("~/Home"));
                        MailHelper.SendEmail(winner.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentW);
                    }
                }
            }
        }
    }
}
