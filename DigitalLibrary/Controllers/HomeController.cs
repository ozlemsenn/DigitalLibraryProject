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
    }
}