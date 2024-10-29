using BankEase.Common;
using BankEase.Common.Messages.AccountMessages;
using BankEase.Data;
using BankEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Controllers
{
    public class AccountController(DatabaseContext context) : Controller
    {
        #region Fields
        private readonly DatabaseContext _context = context;
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            int? nUserId = this.HttpContext.Session.GetInt32(SessionKey.USER_ID);

            if(nUserId is null) return RedirectToAction("Index", "Home");

            List<Account> userAccounts = await _context.Accounts.Where(account => account.CustomerId == nUserId).ToListAsync();

            Customer? customer = await _context.Customers.FindAsync(nUserId);

            this.ViewBag.CustomerFirstName = customer == null ? string.Empty : customer.FirstName;

            List<SelectListItem> accountOptions = userAccounts.Select(account => new SelectListItem
                                                                                 {
                                                                                     Value = account.Id.ToString(),
                                                                                     Text = account.IBAN
                                                                                 }).ToList();
            this.ViewBag.AccountOptions = accountOptions;

            return View();
        }

        public IActionResult AccountSelection(int? nAccountId)
        {
            if(nAccountId is null or 0)
            {
                this.ModelState.AddModelError("account", AccountMessages.AccountNotSelected);
                return RedirectToAction("Index");
            }

            // Benutzerkonto in Session speichern
            this.HttpContext.Session.SetInt32(SessionKey.ACCOUNT_ID, nAccountId.Value);

            return RedirectToAction("Index", "Deposit");
        }
        #endregion
    }
}