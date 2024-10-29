using Microsoft.AspNetCore.Mvc;

namespace BankEase.Controllers
{
    public class DepositController : Controller
    {
        public IActionResult Index()
        {
            return Content("DepositController on Index Page");
        }
    }
}
