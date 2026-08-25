using System;
using System.Linq;
using System.Web.Mvc;
using DigitalLibrary.Models;
using System.Net;
using System.Net.Mail;

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
                    Session["Department"] = user.Department;

                    if (Password.Length == 6 && Password == Password.ToUpper())
                    {
                        TempData["GeciciSifreUyarisi"] = "Şu anda e-postanıza gönderilen <b>geçici şifre</b> ile giriş yaptınız.<br><br>Lütfen güvenliğiniz için profil ayarlarından şifrenizi güncelleyin.";
                    }

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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["Hata"] = "Lütfen e-posta adresinizi girin.";
                return RedirectToAction("Login");
            }

            DigitalLibraryDBEntities1 db = new DigitalLibraryDBEntities1();
            var user = db.Users.FirstOrDefault(u => u.Email == Email);

            if (user != null)
            {
                string yeniSifre = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                user.Password = yeniSifre;

                try
                {
                    string gondericiMail = "ozlemsen381@gmail.com";
                    string gondericiSifre = "nqnheutisvhbqrap";

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(gondericiMail, "DigitalLibrary Sistem");
                    mail.To.Add(Email);
                    mail.Subject = "DigitalLibrary - Şifre Sıfırlama";

                    mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;

                    mail.IsBodyHtml = true;
                    mail.Body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #EAE3D9; border-radius: 10px; max-width: 500px;'>
                    <h2 style='color: #C0392B;'>Şifre Sıfırlama Talebi</h2>
                    <p>Merhaba <b>{user.Name}</b>,</p>
                    <p>Hesabınız için şifre sıfırlama talebinde bulundunuz. Sisteme giriş yapabilmeniz için geçici şifreniz aşağıda belirtilmiştir:</p>
                    <div style='background-color: #FDF3F2; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; color: #D48900; border-radius: 8px; letter-spacing: 2px;'>
                        {yeniSifre}
                    </div>
                    <p style='margin-top: 20px; color: #7F736A; font-size: 12px;'>Lütfen giriş yaptıktan sonra Profil Ayarları kısmından şifrenizi değiştiriniz.</p>
                </div>";

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.EnableSsl = true;

                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(gondericiMail, gondericiSifre);

                    smtp.Send(mail);
                    db.SaveChanges();

                    TempData["SifreSifirlandi"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi! (Lütfen Spam/Gereksiz kutunuzu da kontrol edin).";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    string detayliHata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    TempData["Hata"] = "Mail gönderilirken bir hata oluştu: " + detayliHata;
                    return RedirectToAction("Login");
                }
            }
            else
            {
                TempData["Hata"] = "Sistemde bu e-posta adresine ait bir hesap bulunamadı.";
                return RedirectToAction("Login");
            }
        }
    }
}