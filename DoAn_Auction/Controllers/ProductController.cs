using DoAn_Auction.Filters;
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
                int n = ctx.Auctions.Where(p=>p.Status==true).Count();

                int RecordPerPage = 8;

                int nPages = n / RecordPerPage;

                int m = n % RecordPerPage;
                if (m > 0)
                {
                    nPages++;
                }

                ViewBag.Pages = nPages;

                ViewBag.CurPage = page;

                var list = ctx.Auctions.Where(p => p.Status == true)
                    .OrderBy(p => p.ProID)
                    .Skip((page - 1) * RecordPerPage)
                    .Take(RecordPerPage)
                    .ToList();

                //ViewBag.Pros = from a in ctx.Auctions
                //             join o in ctx.AuctionHistories on a.ProID equals o.ProID
                //             group a by new {a.ProID,a.ProName,a.PriceHighest,a.TimeStart }into g
                //             select new
                //             {
                //                 Count = g.Count()
                //             };
                return View(list);

            }
        }

        // GET: Product/ByCat
        public ActionResult ByCat(int ? id,int page=1)
        {
            if(id.HasValue==false)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                var cat = ctx.Categories
                    .Where(c => c.CatID == id).FirstOrDefault();
                var dssp = new List<Auction>();
                if (cat.Categories.Count > 0)
                {
                    foreach(var catchild in cat.Categories)
                    {
                        var pro = ctx.Auctions
                            .Where(p => p.CatID == catchild.CatID && p.Status == true).ToList();
                        dssp.AddRange(pro);
                    }
                }
                else
                {
                    var pro = ctx.Auctions
                            .Where(p => p.CatID == cat.CatID && p.Status == true).ToList();
                    dssp.AddRange(pro);
                }

                int n = dssp.Count();

                int RecordPerPage = 8;

                int nPages = n / RecordPerPage;

                int m = n % RecordPerPage;
                if (m > 0)
                {
                    nPages++;
                }

                ViewBag.Pages = nPages;

                ViewBag.CurPage = page;

                var list = dssp.OrderBy(p => p.ProID)
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
                //int n = result.Count();

                //int RecordPerPage = 8;

                //int nPages = n / RecordPerPage;

                //int m = n % RecordPerPage;
                //if (m > 0)
                //{
                //    nPages++;
                //}

                //ViewBag.Pages = nPages;

                //ViewBag.CurPage = page;

                //var list = ctx.Auctions
                //    .OrderBy(p => p.ProID)
                //    .Skip((page - 1) * RecordPerPage)
                //    .Take(RecordPerPage)
                //    .ToList();
                ViewBag.AuHis = result.ToList();
                //if (CurrentContext.IsLogged() == true)
                //{
                //    ViewBag.like=;
                //}
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
                if(DateTime.Now.AddMinutes(5)>=model.TimeEnd)
                {
                    if(model.Adjourning==true)
                    {
                        model.TimeEnd = model.TimeEnd.AddMinutes(10);
                        model.Adjourning = false;
                        ctx.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                    }
                }
                if (uPriceSet > model.PriceHighest)
                {
                    var OldHolder = new User();
                    var seller = ctx.Users.Where(u => u.f_ID == model.Seller).FirstOrDefault();
                    string contentO = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                    if(model.Customer!=0)
                    {
                        OldHolder = ctx.Users.Where(u => u.f_ID == model.Customer).FirstOrDefault();
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
                    }
                    if (model.PriceHighest == 0)
                    {
                        model.PriceCurrent = model.PriceCurrent + model.Step;
                    }
                    else
                    {
                        model.PriceCurrent = model.PriceHighest + model.Step;
                    }
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
                        PriceHighest = uPriceSet,
                    };
                    ctx.AuctionHistories.Add(auHis);
                    ctx.SaveChanges();
                    ViewBag.AuHis = result.ToList();

                    var NewHolder = ctx.Users.Where(u => u.f_ID == UID).FirstOrDefault();
                    string contentS = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                    string contentN = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
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
                        ViewBag.ErrorMsg = "Đã có người đặt giá này trước bạn bạn ! Vui lòng đặt giá khác";
                        var auHis = new AuctionHistory
                        {
                            ProID = model.ProID,
                            UserID = UID,
                            Price = model.PriceCurrent,
                            Time = DateTime.Now,
                            Status = true,
                            PriceHighest = uPriceSet,
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
                                PriceHighest = uPriceSet,
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

        // POST: Product/Detail - KickUser
        [CheckLogin]
        [HttpPost]
        public ActionResult KickUser(int? ProID, int? KUserID)
        {
            if (ProID.HasValue == false || KUserID.HasValue == false)
            {
                return RedirectToAction("Index", "Product");
            }
            using (var ctx = new QLDauGiaEntities())
            {
                var auhis = ctx.AuctionHistories
                    .Where(p => p.ProID == ProID && p.UserID == KUserID)
                    .ToList();
                var pro = ctx.Auctions.Where(p => p.ProID == ProID).FirstOrDefault();
                if(CurrentContext.IsLogged()==true)
                {
                    if(CurrentContext.GetCurUser().f_ID!= pro.Seller)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                foreach (var p in auhis)
                {
                    p.Status = false;
                    ctx.Entry(p).State = System.Data.Entity.EntityState.Modified;
                }
                ctx.SaveChanges();
                var au = ctx.AuctionHistories.OrderByDescending(p => p.Price)
                    .Where(p => p.ProID == ProID && p.Status==true).Take(1).FirstOrDefault();
                var seller = ctx.Users.Where(u => u.f_ID == pro.Seller).FirstOrDefault();
                var kicked = ctx.Users.Where(u => u.f_ID == KUserID).FirstOrDefault();

                //Send email to KickedkUser
                string content = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplKick.html"));
                content = content.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                content = content.Replace("{{MsgHeader}}", "Bạn đã bị cấm tham gia đấu giá của một sản phẩm");
                content = content.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                content = content.Replace("{{KName}}", kicked.f_Name);
                content = content.Replace("{{ProName}}", pro.ProName);
                content = content.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                content = content.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                MailHelper.SendEmail(kicked.f_Email, "Cảnh báo về sản phẩm bạn đang đấu giá", content);

                if(pro.Customer==KUserID)
                {
                    if (au != null)
                    {
                        pro.PriceHighest = au.PriceHighest;
                        pro.PriceCurrent = au.Price;
                        pro.Customer = au.UserID;
                        ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();

                        //Send email to HighestUser
                        var huser = ctx.Users.Where(u => u.f_ID == au.UserID).FirstOrDefault();
                        string contentH = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                        contentH = contentH.Replace("{{ColorTitle}}", "#3333ff");
                        contentH = contentH.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                        contentH = contentH.Replace("{{MsgHeader}}", "Bạn đã trờ thành người giữ giá của một sản phẩm");
                        contentH = contentH.Replace("{{Msg}}", "Bạn đã là người giữ giá cao nhất của sản phẩm dưới đấy");
                        contentH = contentH.Replace("{{UName}}", huser.f_Name);
                        contentH = contentH.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                        contentH = contentH.Replace("{{ProName}}", pro.ProName);
                        contentH = contentH.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                        contentH = contentH.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                        MailHelper.SendEmail(huser.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentH);
                    }
                    else
                    {
                        pro.PriceHighest = 0;
                        pro.PriceCurrent = pro.PriceStarting;
                        pro.Customer = 0;
                        ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                    }
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            //return RedirectToAction("Detail", "Product", new { id = ProID });
        }

        // GET: Product/Detail - TimeOver
        [HttpPost]
        public ActionResult TimeOver(int? ProID)
        {
            if (ProID.HasValue == false )
            {
                return RedirectToAction("Index", "Product");
            }
            using (var ctx = new QLDauGiaEntities())
            {
                var pro=ctx.Auctions.Where(p=>p.ProID==ProID).FirstOrDefault();
                if(pro.Status==false)
                {
                    return RedirectToAction("Index", "Product");
                }
                if(pro.Adjourning==true)
                {
                    pro.TimeEnd.AddMinutes(10);
                    ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    return Json(1,JsonRequestBehavior.AllowGet);
                }
                var seller = ctx.Users.Where(u => u.f_ID == pro.Seller).FirstOrDefault();
                string contentSN = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                string contentS = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                string contentW = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                if(pro.Customer==0)
                {
                    pro.Status = false;
                    ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    //Email to Seller
                    contentSN = contentSN.Replace("{{ColorTitle}}", "#ffad33");
                    contentSN = contentSN.Replace("{{Title}}", "Sản phẩm đã đăng");
                    contentSN = contentSN.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                    contentSN = contentSN.Replace("{{UName}}", seller.f_Name);
                    contentSN = contentSN.Replace("{{Msg}}", "Đấu giá kết thúc, không có ai đấu giá sản phẩm của bạn");
                    contentSN = contentSN.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                    contentSN = contentSN.Replace("{{ProName}}", pro.ProName);
                    contentSN = contentSN.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                    contentSN = contentSN.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                    MailHelper.SendEmail(seller.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentSN);
                }
                else
                {
                    pro.Status = false;
                    ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    var winner = ctx.Users.Where(u => u.f_ID == pro.Customer).FirstOrDefault();
                    //Email to Seller
                    contentS = contentS.Replace("{{ColorTitle}}", "#66ff33");
                    contentS = contentS.Replace("{{Title}}", "Sản phẩm đã đăng");
                    contentS = contentS.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                    contentS = contentS.Replace("{{UName}}", seller.f_Name);
                    contentS = contentS.Replace("{{Msg}}", "Đấu giá kết thúc, đã có người chiến thắng sản phẩm của bạn");
                    contentS = contentS.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                    contentS = contentS.Replace("{{ProName}}", pro.ProName);
                    contentS = contentS.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                    contentS = contentS.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                    MailHelper.SendEmail(seller.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentS);
                    //Email to Winner
                    contentW = contentW.Replace("{{ColorTitle}}", "#66ff33");
                    contentW = contentW.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                    contentW = contentW.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                    contentW = contentW.Replace("{{UName}}", winner.f_Name);
                    contentW = contentW.Replace("{{Msg}}", "Đấu giá kết thúc, bạn đã chiến thắng trong một sản phẩm");
                    contentW = contentW.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                    contentW = contentW.Replace("{{ProName}}", pro.ProName);
                    contentW = contentW.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                    contentW = contentW.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                    MailHelper.SendEmail(winner.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentW);
                    return Json(2, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        // POST: Product/Detail - BuyEndBid
        [CheckLogin]
        [HttpPost]
        public ActionResult BuyEndBid(int? ProID)
        {
            if (CurrentContext.IsLogged() == false)
            {
                return RedirectToAction("Register", "Account");
            }
            if (ProID.HasValue == false)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            using (var ctx = new QLDauGiaEntities())
            {
                var UID = 0;
                if (CurrentContext.IsLogged() == true)
                {
                    UID = CurrentContext.GetCurUser().f_ID;
                }
                var pro = ctx.Auctions.Where(p => p.ProID == ProID).FirstOrDefault();
                if (pro.Status == false)
                {
                    return RedirectToAction("Index", "Product");
                }
                pro.Status = false;
                pro.Customer = UID;
                ctx.Entry(pro).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();

                var seller = ctx.Users.Where(u => u.f_ID == pro.Seller).FirstOrDefault();
                var winner = ctx.Users.Where(u => u.f_ID == UID).FirstOrDefault();
                string contentS = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));
                string contentW = System.IO.File.ReadAllText(Server.MapPath("~/assets/mailMsg/tplAuction.html"));

                //Email to Seller
                contentS = contentS.Replace("{{ColorTitle}}", "#66ff33");
                contentS = contentS.Replace("{{Title}}", "Sản phẩm đã đăng");
                contentS = contentS.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                contentS = contentS.Replace("{{UName}}", seller.f_Name);
                contentS = contentS.Replace("{{Msg}}", "Đấu giá kết thúc, đã có người mua sản phẩm của bạn với giá mua ngay");
                contentS = contentS.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                contentS = contentS.Replace("{{ProName}}", pro.ProName);
                contentS = contentS.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                contentS = contentS.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                MailHelper.SendEmail(seller.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentS);
                //Email to Winner
                contentW = contentW.Replace("{{ColorTitle}}", "#66ff33");
                contentW = contentW.Replace("{{Title}}", "Sản phẩm bạn đang đấu giá");
                contentW = contentW.Replace("{{MsgHeader}}", "Đấu giá kết thúc");
                contentW = contentW.Replace("{{UName}}", winner.f_Name);
                contentW = contentW.Replace("{{Msg}}", "Đấu giá kết thúc, bạn đã chiến thắng trong một sản phẩm");
                contentW = contentW.Replace("{{Msg}}", "Đấu giá kết thúc, bạn đã chiến thắng trong một sản phẩm");
                contentW = contentW.Replace("{{SellerName}}", StringUtils.MaHoa(seller.f_Name));
                contentW = contentW.Replace("{{ProName}}", pro.ProName);
                contentW = contentW.Replace("{{ProLink}}", Url.Action("Detail", "Product", new { id = pro.ProID }, this.Request.Url.Scheme).ToString());
                contentW = contentW.Replace("{{Home}}", Url.Action("Index", "Home", null, this.Request.Url.Scheme).ToString());
                MailHelper.SendEmail(winner.f_Email, "Thông báo về sản phẩm bạn đang đấu giá", contentW);
                return Json(1, JsonRequestBehavior.AllowGet);
            }
        }

        [CheckLogin]
        [HttpPost]
        public ActionResult AddDes(int ProID, string DesBonus)
        {
            //var UID = 0;
            if (CurrentContext.IsLogged() == false)
            {
                return RedirectToAction("Detail", "Product", new { id = ProID });
            }
            using (var ctx = new QLDauGiaEntities())
            {
                var bonus = new DesBonu
                {
                    ProID = ProID,
                    DesBonus = DesBonus,
                    Time = DateTime.Now,
                };
                ctx.DesBonus.Add(bonus);
                ctx.SaveChanges();
                return RedirectToAction("Detail", "Product", new { id = ProID });
            }
        }
    }
}