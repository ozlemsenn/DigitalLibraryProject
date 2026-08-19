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
            var user = db.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null)
            {
                if (user.IsActive == true)
                {
                    Session["UserID"] = user.ID;
                    Session["Role"] = user.Role;
                    Session["UserName"] = user.Name;

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    TempData["Hata"] = "Hesabınız askıya alınmıştır. ";
                    return View();
                }
            }
            else
            {
                TempData["Hata"] = "Girdiğiniz e-posta adresi veya şifre hatalı.";
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