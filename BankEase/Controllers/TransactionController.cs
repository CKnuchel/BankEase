using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Controllers
{
    public class TransactionController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
    {
        #region Publics
        public async Task<IActionResult> Index()
        {
            int? nUserId = this.HttpContext.Session.GetInt32(SessionKey.USER_ID);
            int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);

            if(nUserId is null or 0) return RedirectToAction("Index", "Home");
            if(nAccountId is null or 0) return RedirectToAction("Index", "Account");

            Account account = (await context.Accounts.FirstOrDefaultAsync(account => account.Id.Equals(nAccountId.Value)))!;

            AccountViewModel viewModel = new AccountViewModel(this.HttpContext, context) { CurrentSaldo = account.Balance };

            return View(viewModel);
        }

        public async Task<IActionResult> Transfer(string strIBAN, decimal mAmount)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}