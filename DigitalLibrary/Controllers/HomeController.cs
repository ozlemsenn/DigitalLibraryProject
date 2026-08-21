using System.Collections.Generic;
using System;
using System.Linq;
using System.Web.Mvc;
using DigitalLibrary.Filters;

using DigitalLibrary.Models;

namespace DigitalLibrary.Controllers
{
    [AuthFilter] //filtremiz
    public class HomeController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        public ActionResult Index()
        {
            string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";
            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

            List<Categories> kategoriler;

            if (userRole == "Personel")
            {
                kategoriler = db.Categories.Where(c => c.IsAdminOnly == false || c.IsAdminOnly == null).ToList();

                var izinVerilenKategoriIDleri = kategoriler.Select(k => k.ID).ToList();

                ViewBag.Documents = db.Documents
                                      .Where(d => d.CategoryID.HasValue &&
                                                  izinVerilenKategoriIDleri.Contains(d.CategoryID.Value) &&
                                                  (d.IsPrivate == false || d.IsPrivate == null || d.UserID == aktifKullaniciID))
                                      .OrderByDescending(d => d.UploadDate)
                                      .ToList();
            }
            else
            {
                kategoriler = db.Categories.ToList();
                ViewBag.Documents = db.Documents.OrderByDescending(d => d.UploadDate).ToList();
            }

            return View(kategoriler);
        }

        public ActionResult MyDocuments()
        {
            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

            var myDocs = db.Documents.Where(d => d.UserID == aktifKullaniciID).OrderByDescending(d => d.ID).ToList();

            string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";

            if (userRole == "Personel")
            {
                ViewBag.Categories = db.Categories.Where(c => c.IsAdminOnly == false || c.IsAdminOnly == null).ToList();
            }
            else
            {
                ViewBag.Categories = db.Categories.ToList();
            }

            return View(myDocs);
        }
            public ActionResult MyHistory()
        {
            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

            var myLogs = db.Logs
                           .Where(l => l.UserID == aktifKullaniciID)
                           .OrderByDescending(l => l.CreatedAt)
                           .ToList();

            return View(myLogs);
        }
        
        public ActionResult ProfileSettings()
        {
            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

            var aktifKullanici = db.Users.FirstOrDefault(u => u.Role == "Personel");

            return View(aktifKullanici);
        }

        [HttpPost]
        public ActionResult UpdateProfile(int ID, string FullName, string Email, string OldPassword, string NewPassword)
        {
            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();
            var user = db.Users.Find(ID);

            if (user != null)
            {
                user.Name = FullName;
                user.Email = Email;

                if (!string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword))
                {
                    if (user.Password == OldPassword)
                    {
                        user.Password = NewPassword; 
                    }
                    else
                    {
                        TempData["Hata"] = "Mevcut şifrenizi yanlış girdiniz. Profil güncellenemedi!";
                        return RedirectToAction("ProfileSettings");
                    }
                }

                db.SaveChanges();
                TempData["Basari"] = "Profil bilgileriniz başarıyla güncellendi!";
            }
            else
            {
                TempData["Hata"] = "Kullanıcı bulunamadı!";
            }

            return RedirectToAction("ProfileSettings");
        }

        [HttpGet]
        public JsonResult GetMyNotifications()
        {
            try
            {
                int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

                var latestLogsRaw = db.Logs
                                      .Where(l => l.UserID == aktifKullaniciID)
                                      .OrderByDescending(l => l.CreatedAt)
                                      .Take(3)
                                      .ToList();

                var latestLogs = latestLogsRaw.Select(l => new {
                    ActionType = l.ActionType,
                    Description = l.Description,
                    IconClass = l.IconClass ?? "fa-bolt",
                    CreatedAt = l.CreatedAt.HasValue ? l.CreatedAt.Value.ToString("dd MMM HH:mm") : ""
                }).ToList();

                return Json(new { success = true, data = latestLogs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetServerStatus()
        {
            try
            {
                string uploadPath = Server.MapPath("~/App_Data/Uploads/");
                long toplamByte = 0;

                if (System.IO.Directory.Exists(uploadPath))
                {
                    var dirInfo = new System.IO.DirectoryInfo(uploadPath);
                    toplamByte = dirInfo.EnumerateFiles().Sum(file => file.Length);
                }

                double kullanilanMB = toplamByte / 1048576.0;
                double kapasiteMB = 1024.0; 
                double dolulukYuzdesi = (kullanilanMB / kapasiteMB) * 100;

                return Json(new
                {
                    success = true,
                    yuzde = Math.Round(dolulukYuzdesi, 1),
                    kullanilan = Math.Round(kullanilanMB, 2)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetLatestDocument()
        {
            try
            {
                int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";

                var query = db.Documents.AsQueryable();

                // Sadece personelin yetkisi olan (gizli olmayan) klasörlerdeki ve herkese açık belgeleri filtrele
                if (userRole == "Personel")
                {
                    var izinliKategoriler = db.Categories.Where(c => c.IsAdminOnly == false || c.IsAdminOnly == null).Select(c => c.ID).ToList();
                    query = query.Where(d => d.CategoryID.HasValue &&
                                             izinliKategoriler.Contains(d.CategoryID.Value) &&
                                             (d.IsPrivate == false || d.IsPrivate == null || d.UserID == aktifKullaniciID));
                }

                var sonBelge = query.OrderByDescending(d => d.ID).FirstOrDefault();

                if (sonBelge != null)
                {
                    // Uzun başlıkları kırpmak için ufak bir dokunuş
                    string kisaBaslik = sonBelge.Title.Length > 18 ? sonBelge.Title.Substring(0, 18) + "..." : sonBelge.Title;
                    return Json(new { success = true, title = kisaBaslik + sonBelge.FileExtension }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, title = "Henüz belge yok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, title = "Hata oluştu" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}