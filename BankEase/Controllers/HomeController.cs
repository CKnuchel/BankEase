using BankEase.Data;
using BankEase.Messages.HomeMessages;
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
        public IActionResult Login(int? user)
        {
            if(user is null or 0)
            {
                this.ModelState.AddModelError("user", HomeMessages.LoginUserNotSelected);
                return RedirectToAction("Index");
            }

            // Benutzer in der Session speichern
            this.HttpContext.Session.SetInt32("user", user.Value);

            return RedirectToAction("Index", "Account");
        }
        #endregion
    }
}