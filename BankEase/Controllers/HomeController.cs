using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Controllers
{
    public class HomeController(DatabaseContext context) : Controller
    {
        #region Fields
        private readonly DatabaseContext _context = context;
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            // Löschen der allfälligen Sessionwerte
            this.HttpContext.Session.Clear();

            // Laden der Customers
            List<Customer> customers = await _context.Customers.ToListAsync();

            List<SelectListItem> customerOptions = customers.Select(customer => new SelectListItem
                                                                                {
                                                                                    Value = customer.Id.ToString(),
                                                                                    Text = $"{customer.FirstName} {customer.LastName}"
                                                                                }).ToList();

            this.ViewBag.CustomerOptions = customerOptions;

            return View();
        }

        [HttpPost]
        public IActionResult Login(int? nUserId)
        {
            if(nUserId is null or 0)
            {
                this.ModelState.AddModelError("user", HomeMessages.LoginUserNotSelected);
                return RedirectToAction("Index");
            }

            // Benutzer in der Session speichern
            this.HttpContext.Session.SetInt32(SessionKey.USER_ID, nUserId.Value);

            return RedirectToAction("Index", "Account");
        }
        #endregion
    }
}