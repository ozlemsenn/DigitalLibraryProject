using System.Linq;
using System.Web.Mvc;

using DigitalLibrary.Models;

namespace DigitalLibrary.Controllers
{
    public class HomeController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        public ActionResult Index()
        {
            var kategoriler = db.Categories.ToList();
            ViewBag.Documents = db.Documents.OrderByDescending(d => d.UploadDate).ToList();

            return View(kategoriler);
        }
        public ActionResult MyDocuments()
        {
            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

            var myDocs = db.Documents.OrderByDescending(d => d.ID).ToList();

            ViewBag.Categories = db.Categories.ToList();

            return View(myDocs);
        }

        public ActionResult MyHistory()
        {
            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

            var myLogs = db.Logs.OrderByDescending(l => l.ID).Take(50).ToList();

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