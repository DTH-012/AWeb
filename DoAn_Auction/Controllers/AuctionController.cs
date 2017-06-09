using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace DoAn_Auction.Controllers
{
    public class AuctionController : Controller
    {
        // GET: Auction/Add
        public ActionResult Add()
        {
            QLDauGiaEntities ctx = new QLDauGiaEntities();
            ViewBag.Parent = ctx.Categories
                    .Where(c => c.ParentID == 0).ToList();
            return View();
        }

        // Post: Auction/Add
        [HttpPost]
        //Quan trọng: upload picture, form bên view phải có encytype
        public ActionResult Add(AuctionVM avm, HttpPostedFileBase mainPic, IEnumerable<HttpPostedFileBase> subPic)
        {
            QLDauGiaEntities ctx = new QLDauGiaEntities();
            ViewBag.Parent = ctx.Categories
                    .Where(c => c.ParentID == 0).ToList();
            Auction u = new Auction
            {
                ProName = avm.ProName,
                FullDes = avm.FullDes,
                PriceStarting = avm.PriceStarting,
                PriceBuy = avm.PriceBuy,
                Quantity = avm.Quantity,
                CatID = avm.CatID,
                PriceHighest = 0,
                Customer = 0,
                TimeStart = DateTime.Now,
                TimeEnd = DateTime.Now.AddDays(avm.TimeEnd),
                Adjourning = false,
                Status = true,
                Required = avm.Required,
                Step = avm.Step,
                Seller = 1,
                PriceCurrent = avm.PriceStarting,
            };

            using (QLDauGiaEntities atx = new QLDauGiaEntities())
            {

                atx.Auctions.Add(u);
                atx.SaveChanges();
                ViewBag.Success = "Đăng đấu giá thành công";

                // Upload picture
                if (mainPic != null && mainPic.ContentLength > 0)
                {
                    //tạo folder chứa hình Images/sp/[ID product]
                    string spDirPath = Server.MapPath("~/Images/sp");
                    string targetDirPath = Path.Combine(spDirPath, u.ProID.ToString());

                    string spLargeDirPath = Server.MapPath("~/Images/sp/Large");
                    string targetLargeDirPath = Path.Combine(spLargeDirPath, u.ProID.ToString());

                    Directory.CreateDirectory(targetDirPath);
                    Directory.CreateDirectory(targetLargeDirPath);
                    //
                    //copy hình main lên
                    string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                    WebImage imgMain = new WebImage(mainPic.InputStream);
                    WebImage imgMainLarge = imgMain;
                    if (imgMain.Width / 440 > 1 || imgMain.Height / 600 > 1)
                    {
                        int mw = imgMain.Width / 440;
                        int mh = imgMain.Height / 600;
                        imgMain.Resize(imgMain.Width / mw, imgMain.Height / mh);
                    }
                    imgMain.Save(mainFileName, "jpg");
                    //
                    //mainPic.SaveAs(mainFileName);
                    //copy hình zoom main
                    string imgMainLargeName = Path.Combine(targetLargeDirPath, "main.jpg");
                    int smw = 1025 / imgMainLarge.Width;
                    int smh = 1400 / imgMainLarge.Height;
                    imgMainLarge.Resize(imgMainLarge.Width * smw, imgMainLarge.Height * smh);
                    imgMainLarge.Save(imgMainLargeName, "jpg");
                    //
                    //Sub picture
                    int i = 0;
                    foreach (var file in subPic)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            i++;

                            string imgName = Path.Combine(targetDirPath, i.ToString() + ".jpg");
                            WebImage imgFile = new WebImage(file.InputStream);
                            WebImage imgLarge = imgFile;
                            //imgLarge.Equals(imgFile);
                            if (imgFile.Width / 440 > 1 || imgFile.Height / 600 > 1)
                            {
                                int w = imgFile.Width / 440;
                                int h = imgFile.Height / 600;
                                imgFile.Resize(imgFile.Width / w, imgFile.Height / h);
                            }
                            imgFile.Save(imgName, "jpg");
                            //file.SaveAs(imgName);

                            string imgLargeName = Path.Combine(targetLargeDirPath, i.ToString());
                            //WebImage imgLarge = new WebImage(imgFile.);
                            int sw = 1025 / imgLarge.Width;
                            int sh = 1400 / imgLarge.Height;
                            imgLarge.Resize(imgLarge.Width * sw, imgLarge.Height * sh);
                            imgLarge.Save(imgLargeName, "jpg");
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "Đăng hình đấu giá không thành công";
                    return View();
                }


            }
            //QLDauGiaEntities ctx = new QLDauGiaEntities();
            ViewBag.Parent = ctx.Categories
                    .Where(c => c.ParentID == 0).ToList();
            return View();
        }

        public ActionResult AddPicture()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddPicture(HttpPostedFileBase mainPic, IEnumerable<HttpPostedFileBase> subPic)
        {
            if (mainPic != null && mainPic.ContentLength > 0)
            {
                //tạo folder chứa hình Images/sp/[ID product]
                string spDirPath = Server.MapPath("~/Images/sp");
                string targetDirPath = Path.Combine(spDirPath, 1.ToString());

                string spLargeDirPath = Server.MapPath("~/Images/sp/Large");
                string targetLargeDirPath = Path.Combine(spLargeDirPath, 1.ToString());

                Directory.CreateDirectory(targetDirPath);
                Directory.CreateDirectory(targetLargeDirPath);
                //
                //copy hình main lên
                string mainFileName = Path.Combine(targetDirPath, "main.jpg");
                WebImage imgMain = new WebImage(mainPic.InputStream);
                WebImage imgMainLarge = imgMain;
                if (imgMain.Width / 440 > 1 || imgMain.Height / 600 > 1)
                {
                    int mw = imgMain.Width / 440;
                    int mh = imgMain.Height / 600;
                    imgMain.Resize(imgMain.Width / mw, imgMain.Height / mh);
                }
                imgMain.Save(mainFileName, "jpg");
                //
                //mainPic.SaveAs(mainFileName);
                //copy hình zoom main
                string imgMainLargeName = Path.Combine(targetLargeDirPath, "main.jpg");
                int smw = 1025 / imgMainLarge.Width;
                int smh = 1400 / imgMainLarge.Height;
                imgMainLarge.Resize(imgMainLarge.Width*smw, imgMainLarge.Height*smh);
                imgMainLarge.Save(imgMainLargeName, "jpg");
                //
                //Sub picture
                int i = 0;
                foreach (var file in subPic)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        i++;

                        string imgName = Path.Combine(targetDirPath, i.ToString() + ".jpg");
                        WebImage imgFile = new WebImage(file.InputStream);
                        WebImage imgLarge = imgFile;
                        //imgLarge.Equals(imgFile);
                        if (imgFile.Width / 440 > 1 || imgFile.Height / 600 > 1)
                        {
                            int w = imgFile.Width / 440;
                            int h = imgFile.Height / 600;
                            imgFile.Resize(imgFile.Width / w, imgFile.Height / h);
                        }
                        imgFile.Save(imgName, "jpg");
                        //file.SaveAs(imgName);

                        string imgLargeName = Path.Combine(targetLargeDirPath, i.ToString());
                        //WebImage imgLarge = new WebImage(imgFile.);
                        int sw = 1025 / imgLarge.Width;
                        int sh = 1400 / imgLarge.Height;
                        imgLarge.Resize(imgLarge.Width * sw, imgLarge.Height * sh);
                        imgLarge.Save(imgLargeName, "jpg");
                        ViewBag.Success = "thành công";
                    }
                }
            }
            else
            {
                ViewBag.ErrorMsg = "ko thành công";
            }
            return View();
        }
    }
}