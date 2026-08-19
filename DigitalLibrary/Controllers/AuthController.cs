using System.Linq;
using System.Web.Mvc;
using DigitalLibrary.Models;

namespace DigitalLibrary.Controllers
{
    public class AuthController : Controller
    {
        DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password && u.IsActive == true);

            if (user != null)
            {
                Session["UserID"] = user.ID;
                Session["Role"] = user.Role;
                Session["UserName"] = user.Name; 

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Documents", "Admin"); 
                }
                else
                {
                    return RedirectToAction("Index", "Home"); 
                }
            }
            else
            {
                TempData["Hata"] = "E-Posta adresi, şifre hatalı veya hesabınız pasif!";
                return View(); 
            }
        }

        public ActionResult Logout()
        {
            Session.Clear(); 
            Session.Abandon(); 

            return RedirectToAction("Login", "Auth");
        }
    }
}