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
    }
}