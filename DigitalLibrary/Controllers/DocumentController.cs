using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using DigitalLibrary.Models;
using DigitalLibrary.Filters;

namespace DigitalLibrary.Controllers
{
    [AuthFilter]
    public class DocumentController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        [HttpPost]
        public ActionResult Upload(string Title, int CategoryID, HttpPostedFileBase uploadedFile, string Source = "", string Visibility = "Public", bool IsAdminOnly = false)
        {
            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
            string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";

            if (string.IsNullOrEmpty(Source))
            {
                Source = Request.Form["Source"];
            }

            if (string.IsNullOrEmpty(Source) && Request.UrlReferrer != null)
            {
                string geldigiLink = Request.UrlReferrer.ToString().ToLower();
                if (geldigiLink.Contains("/home") || geldigiLink.EndsWith("/"))
                {
                    Source = "Personel";
                }
            }

            var secilenKategori = db.Categories.Find(CategoryID);
            if (userRole == "Personel" && secilenKategori != null && secilenKategori.IsAdminOnly == true)
            {
                TempData["Hata"] = "Bu klasöre dosya yükleme yetkiniz bulunmamaktadır!";
                return Source == "Personel" ? RedirectToAction("Index", "Home") : RedirectToAction("Documents", "Admin");
            }

            if (uploadedFile != null && uploadedFile.ContentLength > 0)
            {
                string[] allowedExtensions = { ".pdf", ".docx", ".xlsx" };
                string fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension) || fileExtension == ".exe")
                {
                    TempData["Hata"] = "Sadece .pdf, .docx veya .xlsx uzantılı dosyalar yükleyebilirsiniz!";
                    return Source == "Personel" ? RedirectToAction("Index", "Home") : RedirectToAction("Documents", "Admin");
                }

                var ayarlar = db.SystemSettings.FirstOrDefault();
                int limitMB = (ayarlar != null && ayarlar.MaxUploadSizeMB > 0) ? ayarlar.MaxUploadSizeMB.Value : 5;
                long limitByte = limitMB * 1024 * 1024;

                if (uploadedFile.ContentLength > limitByte)
                {
                    TempData["Hata"] = "Dosya boyutu " + limitMB + " MB sınırını aşıyor! Limitleri 'Sistem Ayarları'ndan yükseltebilirsiniz.";
                    return Source == "Personel" ? RedirectToAction("Index", "Home") : RedirectToAction("Documents", "Admin");
                }

                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string uploadFolderPath = Server.MapPath("~/App_Data/Uploads/");

                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }

                string savePath = Path.Combine(uploadFolderPath, uniqueFileName);
                uploadedFile.SaveAs(savePath);

                Documents newDoc = new Documents();
                newDoc.Title = Title;
                newDoc.FilePath = "/App_Data/Uploads/" + uniqueFileName;
                newDoc.FileExtension = fileExtension;
                newDoc.CategoryID = CategoryID;
                newDoc.UploadDate = DateTime.Now;
                newDoc.UserID = aktifKullaniciID;

                newDoc.IsPrivate = IsAdminOnly;


                db.Documents.Add(newDoc);

                Logs docLog = new Logs();
                docLog.ActionType = "Yeni Doküman Yüklendi";

                string gizlilikMesajı = IsAdminOnly ? " (Gizli Belge)" : "";
                docLog.Description = "'" + Title + "' isimli belge" + gizlilikMesajı + " sisteme eklendi.";

                docLog.IconClass = "fa-file-upload";
                docLog.CreatedAt = DateTime.Now;
                docLog.UserID = aktifKullaniciID;
                db.Logs.Add(docLog);

                db.SaveChanges();

                TempData["Basari"] = "Doküman başarıyla yüklendi!";
            }
            else
            {
                TempData["Hata"] = "Lütfen yüklemek için bir dosya seçin!";
            }

            if (Source == "Personel")
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Documents", "Admin");
        }

        public ActionResult Download(int id)
        {
            var document = db.Documents.Find(id);
            if (document == null) return HttpNotFound("Doküman bulunamadı!");

            string filePath = Server.MapPath(document.FilePath);
            if (!System.IO.File.Exists(filePath)) return HttpNotFound("Fiziksel dosya sunucuda bulunamadı!");

            return File(filePath, "application/octet-stream", document.Title + document.FileExtension);
        }

        [HttpGet]
        public JsonResult Search(string kelime)
        {
            string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";
            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
            var query = db.Documents.AsQueryable();

            if (userRole == "Personel")
            {
                var izinliKategoriler = db.Categories.Where(c => c.IsAdminOnly == false || c.IsAdminOnly == null).Select(c => c.ID).ToList();

                query = query.Where(d => d.CategoryID.HasValue &&
                                         izinliKategoriler.Contains(d.CategoryID.Value) &&
                                         (d.IsPrivate == false || d.IsPrivate == null || d.UserID == aktifKullaniciID));
            }

            if (!string.IsNullOrEmpty(kelime))
            {
                query = query.Where(d => d.Title.Contains(kelime));
            }

            var hamVeri = query.OrderByDescending(d => d.UploadDate)
                               .Select(d => new { d.ID, d.Title, d.FileExtension, d.UploadDate }).ToList();

            var sonuclar = hamVeri.Select(d => new {
                d.ID,
                d.Title,
                d.FileExtension,
                UploadDateFormated = d.UploadDate.HasValue ? d.UploadDate.Value.ToString("dd.MM.yyyy HH:mm") : ""
            }).ToList();

            return Json(sonuclar, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetByCategory(int id)
        {
            string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";
            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

            var kategoriIdListesi = db.Categories.Where(k => k.ID == id || k.ParentID == id).Select(k => k.ID).ToList();
            var query = db.Documents.Where(d => d.CategoryID.HasValue && kategoriIdListesi.Contains(d.CategoryID.Value));

            if (userRole == "Personel")
            {
                query = query.Where(d => d.IsPrivate == false || d.IsPrivate == null || d.UserID == aktifKullaniciID);
            }

            var hamVeri = query.OrderByDescending(d => d.UploadDate)
                               .Select(d => new { d.ID, d.Title, d.FileExtension, d.UploadDate }).ToList();

            var sonuclar = hamVeri.Select(d => new {
                d.ID,
                d.Title,
                d.FileExtension,
                UploadDateFormated = d.UploadDate.HasValue ? d.UploadDate.Value.ToString("dd.MM.yyyy HH:mm") : ""
            }).ToList();

            return Json(sonuclar, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var document = db.Documents.Find(id);
                if (document == null) return Json(new { success = false, message = "Doküman veritabanında bulunamadı!" });

                int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";

                if (document.UserID != aktifKullaniciID && userRole != "Admin")
                {
                    return Json(new { success = false, message = "Sadece kendi yüklediğiniz belgeleri silebilirsiniz!" });
                }

                string silinenDokumanAdi = document.Title;
                string filePath = Server.MapPath(document.FilePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                db.Documents.Remove(document);

                Logs deleteLog = new Logs();
                deleteLog.ActionType = "Doküman Silindi";
                deleteLog.Description = "'" + silinenDokumanAdi + "' isimli belge arşivden çıkarıldı.";
                deleteLog.IconClass = "fa-trash";
                deleteLog.CreatedAt = DateTime.Now;
                deleteLog.UserID = aktifKullaniciID;
                db.Logs.Add(deleteLog);

                db.SaveChanges();

                return Json(new { success = true, message = "Doküman başarıyla silindi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetDocument(int id)
        {
            var document = db.Documents.Find(id);
            if (document == null) return Json(new { success = false, message = "Döküman bulunamadı" }, JsonRequestBehavior.AllowGet);

            int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
            string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";

            if (document.UserID != aktifKullaniciID && userRole != "Admin")
            {
                return Json(new { success = false, message = "Bu belgeyi görüntüleme yetkiniz yok!" }, JsonRequestBehavior.AllowGet);
            }

            string catName = "Kategorisiz";
            if (document.CategoryID.HasValue)
            {
                var cat = db.Categories.Find(document.CategoryID.Value);
                if (cat != null) catName = cat.Name;
            }

            string uploaderName = "Bilinmeyen Kullanıcı";
            if (document.UserID.HasValue)
            {
                var uploader = db.Users.Find(document.UserID.Value);
                if (uploader != null) uploaderName = uploader.Name;
            }

            var data = new
            {
                document.ID,
                document.Title,
                document.CategoryID,
                CategoryName = catName, 
                document.Description, 
                document.FileExtension, 
                UploadDate = document.UploadDate.HasValue ? document.UploadDate.Value.ToString("dd MMMM yyyy HH:mm") : "-", // Formatlı Tarih
                UploaderName = uploaderName, 
                IsAdminOnly = document.IsPrivate ?? false
            };

            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(int id, string title, int categoryId, bool isAdminOnly = false) 
        {
            try
            {
                var document = db.Documents.Find(id);
                if (document == null) return Json(new { success = false, message = "Güncellenecek doküman bulunamadı." });

                int aktifKullaniciID = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                string userRole = Session["Role"] != null ? Session["Role"].ToString() : "";

                if (document.UserID != aktifKullaniciID && userRole != "Admin")
                {
                    return Json(new { success = false, message = "Sadece kendi yüklediğiniz belgeleri güncelleyebilirsiniz!" });
                }

                var secilenKategori = db.Categories.Find(categoryId);
                if (userRole == "Personel" && secilenKategori != null && secilenKategori.IsAdminOnly == true)
                {
                    return Json(new { success = false, message = "Belgeyi bu klasöre taşıma yetkiniz yok!" });
                }

                document.Title = title;
                document.CategoryID = categoryId;

                document.IsPrivate = isAdminOnly;

                Logs updateLog = new Logs();
                updateLog.ActionType = "Doküman Güncellendi";

                string gizlilikMesajı = isAdminOnly ? " (Gizli Belge Yapıldı)" : "";
                updateLog.Description = "'" + title + "' isimli belgenin bilgileri güncellendi" + gizlilikMesajı + ".";

                updateLog.IconClass = "fa-file-signature";
                updateLog.CreatedAt = DateTime.Now;
                updateLog.UserID = aktifKullaniciID;
                db.Logs.Add(updateLog);

                db.SaveChanges();

                return Json(new { success = true, message = "Doküman başarıyla güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Güncelleme sırasında hata oluştu: " + ex.Message });
            }
        }
    }
}