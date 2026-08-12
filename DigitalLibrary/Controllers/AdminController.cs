using System.Web.Mvc;

namespace DigitalLibrary.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}