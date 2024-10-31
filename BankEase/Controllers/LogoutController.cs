using Microsoft.AspNetCore.Mvc;

namespace BankEase.Controllers
{
    public class LogoutController : Controller
    {
        #region Publics
        public IActionResult Logout()
        {
            this.HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}