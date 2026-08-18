using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using DigitalLibrary.Models;

namespace DigitalLibrary.Controllers
{
    public class DocumentController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        [HttpPost]
        public ActionResult Upload(string Title, int CategoryID, HttpPostedFileBase uploadedFile, string Source = "")
        {
            // 0. KESİN TESPİT: Kullanıcı Nereden Geldi?
            if (string.IsNullOrEmpty(Source))
            {
                Source = Request.Form["Source"];
            }

            // Hala bulamadıysa tarayıcı URL'sine bak
            if (string.IsNullOrEmpty(Source) && Request.UrlReferrer != null)
            {
                string geldigiLink = Request.UrlReferrer.ToString().ToLower();
                if (geldigiLink.Contains("/home") || geldigiLink.EndsWith("/"))
                {
                    Source = "Personel"; // İngilizce olan kısmı düzelttik
                }
            }

            if (uploadedFile != null && uploadedFile.ContentLength > 0)
            {
                string[] allowedExtensions = { ".pdf", ".docx", ".xlsx" };
                string fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();

                // 1. UZANTI KONTROLÜ
                if (!allowedExtensions.Contains(fileExtension) || fileExtension == ".exe")
                {
                    TempData["Hata"] = "Sadece .pdf, .docx veya .xlsx uzantılı dosyalar yükleyebilirsiniz!";

                    if (Source == "Personel") return RedirectToAction("Index", "Home");
                    return RedirectToAction("Documents", "Admin");
                }

                // 2. BOYUT KONTROLÜ
                var ayarlar = db.SystemSettings.FirstOrDefault();
                int limitMB = (ayarlar != null && ayarlar.MaxUploadSizeMB > 0) ? ayarlar.MaxUploadSizeMB.Value : 5;
                long limitByte = limitMB * 1024 * 1024;

                if (uploadedFile.ContentLength > limitByte)
                {
                    TempData["Hata"] = "Dosya boyutu " + limitMB + " MB sınırını aşıyor! Limitleri 'Sistem Ayarları'ndan yükseltebilirsiniz.";

                    if (Source == "Personel") return RedirectToAction("Index", "Home");
                    return RedirectToAction("Documents", "Admin");
                }

                // 3. DOSYAYI SUNUCUYA KAYDETME
                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string uploadFolderPath = Server.MapPath("~/App_Data/Uploads/");

                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }

                string savePath = Path.Combine(uploadFolderPath, uniqueFileName);
                uploadedFile.SaveAs(savePath);

                // 4. VERİTABANINA DOKÜMAN EKLEME
                Documents newDoc = new Documents();
                newDoc.Title = Title;
                newDoc.FilePath = "/App_Data/Uploads/" + uniqueFileName;
                newDoc.FileExtension = fileExtension;
                newDoc.CategoryID = CategoryID;
                newDoc.UploadDate = DateTime.Now;

                db.Documents.Add(newDoc);

                // 5. LOGLAMA
                Logs docLog = new Logs();
                docLog.ActionType = "Yeni Doküman Yüklendi";
                docLog.Description = "'" + Title + "' isimli belge sisteme eklendi.";
                docLog.IconClass = "fa-file-upload";
                docLog.CreatedAt = DateTime.Now;
                db.Logs.Add(docLog);

                db.SaveChanges();

                TempData["Basari"] = "Doküman başarıyla yüklendi!";
            }
            else
            {
                TempData["Hata"] = "Lütfen yüklemek için bir dosya seçin!";
            }

            // 6. SONUÇ YÖNLENDİRMESİ
            if (Source == "Personel")
            {
                return RedirectToAction("Index", "Home"); // Personelse kendi sayfasına dönsün
            }

            return RedirectToAction("Documents", "Admin"); // Admin ise panele dönsün
        }

        public ActionResult Download(int id)
        {
            var document = db.Documents.Find(id);

            if (document == null)
            {
                return HttpNotFound("Doküman bulunamadı!");
            }

            string filePath = Server.MapPath(document.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("Fiziksel dosya sunucuda bulunamadı!");
            }

            return File(filePath, "application/octet-stream", document.Title + document.FileExtension);
        }

        [HttpGet]
        public JsonResult Search(string kelime)
        {
            var query = db.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(kelime))
            {
                query = query.Where(d => d.Title.Contains(kelime));
            }

            var hamVeri = query.OrderByDescending(d => d.UploadDate)
                               .Select(d => new {
                                   d.ID,
                                   d.Title,
                                   d.FileExtension,
                                   d.UploadDate
                               }).ToList();

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
            var kategoriIdListesi = db.Categories
                                      .Where(k => k.ID == id || k.ParentID == id)
                                      .Select(k => k.ID)
                                      .ToList();

            var query = db.Documents.Where(d => d.CategoryID.HasValue && kategoriIdListesi.Contains(d.CategoryID.Value));

            var hamVeri = query.OrderByDescending(d => d.UploadDate)
                               .Select(d => new {
                                   d.ID,
                                   d.Title,
                                   d.FileExtension,
                                   d.UploadDate
                               }).ToList();

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

                if (document == null)
                {
                    return Json(new { success = false, message = "Doküman veritabanında bulunamadı!" });
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

            if (document == null)
            {
                return Json(new { success = false, message = "Döküman bulunamadı" }, JsonRequestBehavior.AllowGet);
            }

            var data = new
            {
                document.ID,
                document.Title,
                document.CategoryID
            };

            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(int id, string title, int categoryId)
        {
            try
            {
                var document = db.Documents.Find(id);

                if (document == null)
                {
                    return Json(new { success = false, message = "Güncellenecek doküman bulunamadı." });
                }

                document.Title = title;
                document.CategoryID = categoryId;

                Logs updateLog = new Logs();
                updateLog.ActionType = "Doküman Güncellendi";
                updateLog.Description = "'" + title + "' isimli belgenin bilgileri güncellendi.";
                updateLog.IconClass = "fa-file-signature"; 
                updateLog.CreatedAt = DateTime.Now;
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