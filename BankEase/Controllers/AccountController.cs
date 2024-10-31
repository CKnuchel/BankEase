using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankEase.Controllers
{
    public class AccountController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
    {
        #region Fields
        private readonly SessionService _sessionService = new(httpContextAccessor);
        private readonly AccountService _accountService = new(context);
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            // Benutzer-ID in der Sitzung validieren
            if(!_sessionService.IsUserSessionValid(out int? nUserId))
                return RedirectToAction("Index", "Home");

            // Benutzerkonten abrufen
            List<Account> userAccounts = await _accountService.GetAccountsByCustomerId(nUserId!.Value);

            // Kundeninformation für die Anzeige laden
            Customer? customer = await _accountService.GetCustomerById(nUserId.Value);

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