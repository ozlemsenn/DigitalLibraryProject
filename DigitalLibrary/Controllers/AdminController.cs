using System;
using System.Linq;
using System.Web.Mvc;
using DigitalLibrary.Models;
using DigitalLibrary.Filters;

namespace DigitalLibrary.Controllers
{
    [AuthFilter]
    public class AdminController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        public ActionResult Index()
        {
            ViewBag.TotalDocuments = db.Documents.Count();
            ViewBag.TotalCategories = db.Categories.Count();

            ViewBag.TotalUsers = db.Users.Count();

            var sonDokuman = db.Documents.OrderByDescending(d => d.ID).FirstOrDefault();
            if (sonDokuman != null)
            {
                ViewBag.LastDocument = sonDokuman.Title;
            }
            else
            {
                ViewBag.LastDocument = "Henüz döküman yok.";
            }

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

            ViewBag.KullanilanMB = Math.Round(kullanilanMB, 2);
            ViewBag.KapasiteMB = 1024;
            ViewBag.DolulukYuzdesi = Math.Round(dolulukYuzdesi, 1);

            ViewBag.RecentLogs = db.Logs.OrderByDescending(x => x.CreatedAt).Take(4).ToList();

            return View();
        }

        public ActionResult Users()
        {
            var kullanicilar = db.Users.ToList();
            return View(kullanicilar);
        }

        [HttpPost]
        public ActionResult AddUser(string Name, string Email, string Password, string Role, string Department)
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

                yeniPersonel.Department = Department;

                yeniPersonel.IsActive = true;

                db.Users.Add(yeniPersonel);

                Logs log = new Logs();
                log.ActionType = "Yeni Personel Eklendi";
                string depBilgi = string.IsNullOrEmpty(Department) ? "" : " (" + Department + ")";
                log.Description = "'" + Name + "'" + depBilgi + " isimli personel sisteme dahil edildi.";
                log.IconClass = "fa-user-plus";
                log.CreatedAt = DateTime.Now;

                int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                if (aktifKullaniciID > 0) log.UserID = aktifKullaniciID;

                db.Logs.Add(log);
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
                string silinenAd = silinecekKullanici.Name;

                db.Users.Remove(silinecekKullanici);

                Logs log = new Logs();
                log.ActionType = "Personel Silindi";
                log.Description = "'" + silinenAd + "' isimli personel sistemden kalıcı olarak silindi.";
                log.IconClass = "fa-trash";
                log.CreatedAt = DateTime.Now;
                db.Logs.Add(log);

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
                    Department = user.Department,
                    DeactivationReason = user.DeactivationReason
                }, JsonRequestBehavior.AllowGet);
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditUser(int? ID, string Name, string Email, string Password, string Role, string Department)
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

                    guncellenecekKullanici.Department = Department;

                    Logs log = new Logs();
                    log.ActionType = "Personel Bilgileri Güncellendi";
                    log.Description = "'" + Name + "' isimli personelin bilgileri değiştirildi.";
                    log.IconClass = "fa-user-edit";
                    log.CreatedAt = DateTime.Now;

                    int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                    if (aktifKullaniciID > 0) log.UserID = aktifKullaniciID;

                    db.Logs.Add(log);
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

                    Logs log = new Logs();
                    log.ActionType = kullanici.IsActive ? "Personel Aktifleştirildi" : "Personel Pasife Alındı";
                    log.Description = kullanici.IsActive
                                      ? "'" + kullanici.Name + "' kullanıcısı tekrar aktif edildi."
                                      : "'" + kullanici.Name + "' kullanıcısının sistem erişimi kesildi.";
                    log.IconClass = kullanici.IsActive ? "fa-user-check" : "fa-user-slash";
                    log.CreatedAt = DateTime.Now;

                    db.Logs.Add(log);
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

        public ActionResult Documents()
        {
            ViewBag.Categories = db.Categories.ToList();

            var dokumanlar = db.Documents.OrderByDescending(d => d.ID).ToList();
            return View(dokumanlar);
        }

        public ActionResult Categories()
        {
            ViewBag.AnaKategoriler = db.Categories.Where(k => k.ParentID == null).ToList();

            var kategoriler = db.Categories
                                .OrderBy(k => k.ParentID == null ? k.ID : k.ParentID)
                                .ThenBy(k => k.ID)
                                .ToList();

            return View(kategoriler);
        }

        [HttpPost]
        public ActionResult AddCategory(string Name, int? ParentID, bool IsAdminOnly = false)
        {
            try
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return Json(new { success = false, message = "Kategori adı boş bırakılamaz!" });
                }

                Categories yeniKategori = new Categories();
                yeniKategori.Name = Name;
                yeniKategori.ParentID = ParentID; 
                yeniKategori.IsAdminOnly = IsAdminOnly;

                db.Categories.Add(yeniKategori);

                Logs log = new Logs();
                log.ActionType = ParentID == null ? "Yeni Ana Kategori Eklendi" : "Yeni Alt Kategori Eklendi";
                string gizlilikDurumu = IsAdminOnly ? " (Gizli Klasör)" : "";
                log.Description = "'" + Name + "' isimli kategori arşive eklendi.";
                log.IconClass = ParentID == null ? "fa-folder-plus" : "fa-code-branch";
                log.CreatedAt = DateTime.Now;
                db.Logs.Add(log);

                db.SaveChanges();

                return Json(new { success = true, message = "Kategori başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistemsel hata: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            try
            {
                var kategori = db.Categories.Find(id);
                if (kategori != null)
                {
                    string silinenAd = kategori.Name;
                    db.Categories.Remove(kategori);

                    Logs log = new Logs();
                    log.ActionType = "Kategori Silindi";
                    log.Description = "'" + silinenAd + "' kategorisi sistemden kaldırıldı.";
                    log.IconClass = "fa-folder-minus";
                    log.CreatedAt = DateTime.Now;
                    db.Logs.Add(log);

                    db.SaveChanges();
                    return Json(new { success = true, message = "Kategori başarıyla silindi." });
                }
                return Json(new { success = false, message = "Kategori bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İşlem sırasında hata: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetCategory(int id)
        {
            var kategori = db.Categories.Find(id);
            if (kategori != null)
            {
                return Json(new { ID = kategori.ID, Name = kategori.Name, ParentID = kategori.ParentID, IsAdminOnly = kategori.IsAdminOnly }, JsonRequestBehavior.AllowGet);
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditCategory(int ID, string Name, int? ParentID, bool IsAdminOnly = false) 
        {
            try
            {
                var kategori = db.Categories.Find(ID);
                if (kategori != null)
                {
                    string eskiAd = kategori.Name;
                    kategori.Name = Name;
                    kategori.ParentID = ParentID;
                    kategori.IsAdminOnly = IsAdminOnly; 

                    Logs log = new Logs();
                    log.ActionType = "Kategori Güncellendi";
                    string gizlilikDurumu = IsAdminOnly ? " (Gizli Klasör olarak ayarlandı)" : "";
                    log.Description = "'" + eskiAd + "' kategorisi güncellendi" + gizlilikDurumu + ".";
                    log.IconClass = "fa-edit";
                    log.CreatedAt = DateTime.Now;

                    int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                    if (aktifKullaniciID > 0) log.UserID = aktifKullaniciID;

                    db.Logs.Add(log);

                    db.SaveChanges();
                    return Json(new { success = true, message = "Kategori başarıyla güncellendi." });
                }
                return Json(new { success = false, message = "Kategori bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Settings()
        {
            var ayarlar = db.SystemSettings.FirstOrDefault();

            if (ayarlar == null)
            {
                ayarlar = new SystemSettings();
                ayarlar.CompanyName = "Kurumsal Doküman Merkezi";
                ayarlar.MaxUploadSizeMB = 5;
                ayarlar.IsMaintenanceMode = false;

                db.SystemSettings.Add(ayarlar);
                db.SaveChanges();
            }

            return View(ayarlar);
        }

        [HttpPost]
        public ActionResult UpdateSettings(int ID, string CompanyName, int MaxUploadSizeMB, bool IsMaintenanceMode)
        {
            try
            {
                var ayar = db.SystemSettings.Find(ID);
                if (ayar != null)
                {
                    ayar.CompanyName = CompanyName;
                    ayar.MaxUploadSizeMB = MaxUploadSizeMB;
                    ayar.IsMaintenanceMode = IsMaintenanceMode;

                    Logs log = new Logs();
                    log.ActionType = "Sistem Ayarları Güncellendi";
                    log.Description = "Sistem yapılandırma konfigürasyonları yönetici tarafından değiştirildi.";
                    log.IconClass = "fa-cogs"; 
                    log.CreatedAt = DateTime.Now;
                    db.Logs.Add(log);

                    db.SaveChanges();

                    return Json(new { success = true, message = "Sistem ayarları başarıyla güncellendi." });
                }
                return Json(new { success = false, message = "Ayar kaydı bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetLatestNotifications()
        {
            try
            {
                var latestLogsRaw = db.Logs.OrderByDescending(l => l.CreatedAt).Take(3).ToList();

                var latestLogs = latestLogsRaw.Select(l => new {
                    ActionType = l.ActionType,
                    Description = l.Description,
                    IconClass = l.IconClass ?? "fa-bolt", 
                    CreatedAt = l.CreatedAt.HasValue ? l.CreatedAt.Value.ToString("dd MMM HH:mm") : "",
                    UserName = l.Users != null ? l.Users.Name : "Sistem"
                }).ToList();

                return Json(new { success = true, data = latestLogs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}