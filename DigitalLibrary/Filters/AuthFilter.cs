using System.Web.Mvc;
using System.Web.Routing;

namespace DigitalLibrary.Filters
{
    // Bu sınıfın bir "Filtre" olabilmesi için ActionFilterAttribute'tan miras alması gerekir
    public class AuthFilter : ActionFilterAttribute
    {
        // OnActionExecuting: "Sayfa Yüklenmeden HEMEN ÖNCE araya gir ve şu işlemleri yap" demektir.
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1. KONTROL: Kullanıcı giriş yapmış mı? (Session var mı?)
            if (filterContext.HttpContext.Session["UserID"] == null)
            {
                // Giriş yapmamış! Hemen Login sayfasına yönlendir.
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "controller", "Auth" }, { "action", "Login" } }
                );
                return; // Aşağıdaki kodları okumadan işlemi kes
            }

            // 2. KONTROL: Yetki Kontrolü (Admin mi, Personel mi?)
            string role = filterContext.HttpContext.Session["Role"].ToString();
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            // Eğer girmeye çalıştığı sayfa "Admin" sayfasıysa AMA rolü Admin değilse
            if (controllerName == "Admin" && role != "Admin")
            {
                // Yakalandın! Personel admin sayfasına giremez. Onu kendi kütüphanesine yolla.
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } }
                );
            }

            base.OnActionExecuting(filterContext);
        }
    }
}