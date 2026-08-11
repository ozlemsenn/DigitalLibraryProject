using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalLibrary.Models;

namespace DigitalLibrary.Controllers
{
    public class DocumentController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        [HttpPost]
        public ActionResult Upload(string Title, int CategoryID, HttpPostedFileBase uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.ContentLength > 0)
            {
                string[] allowedExtensions = { ".pdf", ".docx", ".xlsx" };
                string fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension) || fileExtension == ".exe")
                {
                    TempData["Hata"] = "Sadece .pdf, .docx veya .xlsx uzantılı dosyalar yükleyebilirsiniz!";
                    return RedirectToAction("Index", "Home");
                }

                if (uploadedFile.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["Hata"] = "Dosya boyutu 5 MB'tan büyük olamaz!";
                    return RedirectToAction("Index", "Home");
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

                db.Documents.Add(newDoc);
                db.SaveChanges();

                TempData["Basari"] = "Doküman başarıyla yüklendi!";
            }
            else
            {
                TempData["Hata"] = "Lütfen yüklemek için bir dosya seçin!";
            }

            return RedirectToAction("Index", "Home");
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
    }
}