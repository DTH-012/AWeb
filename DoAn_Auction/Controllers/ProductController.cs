using DoAn_Auction.Helpers;
using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product/Index
        public ActionResult Index(int page = 1)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                int n = ctx.Auctions.Count();

                int RecordPerPage = 12;

                int nPages = n / RecordPerPage;

                int m = n % RecordPerPage;
                if (m > 0)
                {
                    nPages++;
                }

                ViewBag.Pages = nPages;

                ViewBag.CurPage = page;

                var list = ctx.Auctions
                    .OrderBy(p => p.ProID)
                    .Skip((page - 1) * RecordPerPage)
                    .Take(RecordPerPage)
                    .ToList();
                return View(list);

            }
        }

        // GET: Product/Detail
        public ActionResult Detail(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("Index", "Home");
            }


            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Auctions
                    .Where(p => p.ProID == id)
                    .FirstOrDefault();
                var result = from u in ctx.Users
                             join ah in ctx.AuctionHistories on u.f_ID equals ah.UserID
                             where ah.ProID==id && ah.Status==true
                             //into g
                             select new AuctionHistoryVM
                             {
                                 ProID=ah.ProID,UserID=ah.UserID,Time=ah.Time,UserName=u.f_Name,Price=ah.Price,
                             };
                ViewBag.AuHis = result.ToList();
                return View(model);
            }
        }
		
		// POST: Product/Detail - Đấu giá
        [HttpPost]
		public ActionResult Detail(int ProID, string PriceSet)
        {
            ViewBag.ContentS = null;
            ViewBag.ContentN = null;
            ViewBag.ContentO = null;
            if (CurrentContext.IsLogged() == false)
            {
                return View();
            }
            int UID = (int)CurrentContext.GetCurUser().f_ID;

            Decimal uPriceSet = Convert.ToDecimal(PriceSet);
            using (var ctx = new QLDauGiaEntities())
            {
                var model = ctx.Auctions
                    .Where(p => p.ProID == ProID)
                    .FirstOrDefault();

                var result = from u in ctx.Users
                             join ah in ctx.AuctionHistories on u.f_ID equals ah.UserID
                             where ah.ProID == ProID && ah.Status==true
                             //into g
                             select new AuctionHistoryVM
                             {
                                 ProID=ah.ProID,
                                 UserID=ah.UserID,
                                 Time = ah.Time,
                                 UserName = u.f_Name,
                                 Price = ah.Price,
                             };
                if (uPriceSet > model.PriceHighest)
                {
                    var OldHolder = ctx.Users.Where(u => u.f_ID == model.Customer).FirstOrDefault();
                    model.PriceCurrent = model.PriceHighest + model.Step;
                    model.PriceHighest = uPriceSet;
                    model.Customer = UID;
                    ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    ViewBag.Success = "Đặt giá thành công ! Bạn đang giữ giá cao nhất";
                    var auHis = new AuctionHistory
                    {
                        ProID = model.ProID,
                        UserID = UID,
                        Price = model.PriceCurrent,
                        Time=DateTime.Now,
                        Status=true,
                    };
                    ctx.AuctionHistories.Add(auHis);
                    ctx.SaveChanges();
                    ViewBag.AuHis = result.ToList();

                    var seller = ctx.Users.Where(u => u.f_ID == model.Seller).FirstOrDefault();
                    var NewHolder = ctx.Users.Where(u => u.f_ID == UID).FirstOrDefault();
                    string contentS = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                    string contentN = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                    string contentO = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                    //Email to seller
                    contentS = contentS.Replace("{{ColorTitle}}", "#3333ff");
                    contentS = contentS.Replace("{{Title}}", "Thay đổi về sản phẩm của bạn");
                    contentS = contentS.Replace("{{MsgHeader}}", "Sản phẩm của bạn đã có người giữ giá mới");
                    contentS = contentS.Replace("{{UName}}", seller.f_Name);
                    contentS = contentS.Replace("{{Msg}}", "Đã có sự thay đổi về người giữ giá trong sản phẩm của bạn\nChúc mừng, sản phẩm của bạn được nâng lên giá cao hơn");
                    contentS = contentS.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                    contentS = contentS.Replace("{{ProName}}", model.ProName);
                    contentS = contentS.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = model.ProID }, this.Request.Url.Scheme).ToString());
                    contentS = contentS.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                    MailHelper.SendEmail(seller.f_Email, "Thông báo về sản phẩm của bạn", contentS);
                    //Email to NewHolder
                    contentN = contentN.Replace("{{ColorTitle}}", "#3333ff");
                    contentN = contentN.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                    contentN = contentN.Replace("{{MsgHeader}}", "Bạn đã trờ thành người giữ giá của một sản phẩm");
                    contentN = contentN.Replace("{{UName}}", NewHolder.f_Name);
                    contentN = contentN.Replace("{{Msg}}", "Bạn đã là người giữ giá cao nhất của sản phẩm dưới đấy");
                    contentN = contentN.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                    contentN = contentN.Replace("{{ProName}}", model.ProName);
                    contentN = contentN.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = model.ProID }, this.Request.Url.Scheme).ToString());
                    contentN = contentN.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                    MailHelper.SendEmail(NewHolder.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentN);
                    //Email to OldHolder
                    contentO = contentO.Replace("{{ColorTitle}}", "#ffad33");
                    contentO = contentO.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                    contentO = contentO.Replace("{{MsgHeader}}", "Bạn đã bị cướp giá trong một sản phẩm");
                    contentO = contentO.Replace("{{UName}}", OldHolder.f_Name);
                    contentO = contentO.Replace("{{Msg}}", "Đã có người trả giá cao hơn bạn trong sản phẩm dưới đấy");
                    contentO = contentO.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                    contentO = contentO.Replace("{{ProName}}", model.ProName);
                    contentO = contentO.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = model.ProID }, this.Request.Url.Scheme).ToString());
                    contentO = contentO.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                    MailHelper.SendEmail(OldHolder.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentO);
                    ViewBag.ContentS = contentS;
                    ViewBag.ContentN = contentN;
                    ViewBag.ContentO = contentO;
                    return RedirectToAction("Index", "Notification", new {Input=model.ProID});
                }
                else
                {
                    if (uPriceSet == model.PriceHighest)
                    {
                        if (ctx.AuctionHistories.Any(ah => ah.ProID == model.ProID && ah.UserID == UID && ah.Price == uPriceSet))
                        {
                            ViewBag.ErrorMsg = "Bạn đã đặt trùng giá cho sản phẩm này ! Vui lòng đặt giá khác";
                            return View(model);
                        }
                        model.PriceCurrent = uPriceSet;
                        ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        ViewBag.ErrorMsg = "Đã có người đặt giá trước bạn bạn ! Vui lòng đặt giá khác";
                        var auHis = new AuctionHistory
                        {
                            ProID = model.ProID,
                            UserID = UID,
                            Price = model.PriceCurrent,
                            Time = DateTime.Now,
                            Status = true,
                        };
                        ctx.AuctionHistories.Add(auHis);
                        ctx.SaveChanges();
                        ViewBag.AuHis = result.ToList();
                        ViewBag.ContentS = null;
                        ViewBag.ContentN = null;
                        ViewBag.ContentO = null;
                        return View(model);

                    }
                    else
                    {
                        if (uPriceSet > model.PriceCurrent)
                        {
                            model.PriceCurrent = uPriceSet + model.Step;
                            ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                            ctx.SaveChanges();
                            ViewBag.ErrorMsg = "Đã có người đặt giá cao hơn bạn ! Vui lòng đặt giá khác";
                            var auHis = new AuctionHistory
                            {
                                ProID = model.ProID,
                                UserID = UID,
                                Price = model.PriceCurrent,
                                Time = DateTime.Now,
                                Status = true,
                            };
                            ctx.AuctionHistories.Add(auHis);
                            ctx.SaveChanges();
                            ViewBag.AuHis = result.ToList();
                            ViewBag.ContentS = null;
                            ViewBag.ContentN = null;
                            ViewBag.ContentO = null;
                            return View(model);
                        }
                    }
                }

                ViewBag.ContentS = null;
                ViewBag.ContentN = null;
                ViewBag.ContentO = null;
                ViewBag.AuHis = result.ToList();
                return View(model);
            }
        } 

    }
}