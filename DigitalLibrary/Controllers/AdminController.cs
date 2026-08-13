using System;
using System.Linq;
using System.Web.Mvc;
using DigitalLibrary.Models;

namespace DigitalLibrary.Controllers
{
    public class AdminController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Users()
        {
            var kullanicilar = db.Users.ToList();

            return View(kullanicilar);
        }


        [HttpPost]
        public ActionResult AddUser(string Name, string Email, string Password, string Role)
        {
            try
            {
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    return Json(new { success = false, message = "Lütfen tüm alanları doldurun!" });
                }

                Users yeniPersonel = new Users();
                yeniPersonel.Name = Name;
                yeniPersonel.Email = Email;
                yeniPersonel.Password = Password;
                yeniPersonel.Role = Role;
                yeniPersonel.IsActive = true; 

                db.Users.Add(yeniPersonel);
                db.SaveChanges();

                return Json(new { success = true, message = "Yeni personel sisteme başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistemsel bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            var silinecekKullanici = db.Users.Find(id);
            if (silinecekKullanici != null)
            {
                db.Users.Remove(silinecekKullanici);
                db.SaveChanges();
                return Json(new { success = true, message = "Personel sistemden başarıyla silindi." });
            }
            return Json(new { success = false, message = "Silinecek personel bulunamadı." });
        }

        [HttpGet]
        public ActionResult GetUser(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                return Json(new
                {
                    ID = user.ID,
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    Role = user.Role,
                    DeactivationReason = user.DeactivationReason
                }, JsonRequestBehavior.AllowGet);
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditUser(int? ID, string Name, string Email, string Password, string Role)
        {
            try
            {
                if (ID == null)
                {
                    return Json(new { success = false, message = "Güncellenecek personelin kimlik numarası (ID) bulunamadı!" });
                }

                var guncellenecekKullanici = db.Users.Find(ID);
                if (guncellenecekKullanici != null)
                {
                    guncellenecekKullanici.Name = Name;
                    guncellenecekKullanici.Email = Email;
                    guncellenecekKullanici.Password = Password;
                    guncellenecekKullanici.Role = Role;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Personel bilgileri başarıyla güncellendi." });
                }

                return Json(new { success = false, message = "Güncellenecek personel bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistemsel bir hata oluştu: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult ToggleUserStatus(int id, string reason = "")
        {
            try
            {
                var kullanici = db.Users.Find(id);
                if (kullanici != null)
                {
                    kullanici.IsActive = !kullanici.IsActive;

                    if (!kullanici.IsActive)
                    {
                        kullanici.DeactivationReason = reason; 
                    }
                    else
                    {
                        kullanici.DeactivationReason = null; 
                    }

                    db.SaveChanges();

                    string mesaj = kullanici.IsActive ? "Personel hesabı başarıyla aktifleştirildi." : "Personel hesabı pasife alındı.";
                    return Json(new { success = true, message = mesaj });
                }
                return Json(new { success = false, message = "İşlem yapılacak personel bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistemsel bir hata oluştu: " + ex.Message });
            }
        }
    }
}