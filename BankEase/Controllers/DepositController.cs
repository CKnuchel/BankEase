using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankEase.Controllers
{
    public class DepositController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
    {
        #region Fields
        private readonly AccountViewModel _accountViewModel = new(httpContextAccessor.HttpContext!, context);
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            int? nUserId = this.HttpContext.Session.GetInt32(SessionKey.USER_ID);
            int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);

            if(nUserId is null or 0) return RedirectToAction("Index", "Home");
            if(nAccountId is null or 0) return RedirectToAction("Index", "Account");

            Account? account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == nAccountId);
            if(account == null) return RedirectToAction("Index", "Account");

            AccountViewModel viewModel = new AccountViewModel(this.HttpContext, context) { CurrentSaldo = account.Balance };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(decimal mAmount)
        {
            if(mAmount <= 0m)
            {
                AccountViewModel viewModel = await _accountViewModel.WithMessage(DepositMessages.DepositAmountMustBeGreaterThanZero, isErrorMessage: true);
                return View("Index", viewModel);
            }

            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);
                if(nAccountId is null or 0) return RedirectToAction("Index", "Account");

                Account? account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == nAccountId);
                if(account == null)
                {
                    AccountViewModel viewModel = await _accountViewModel.WithMessage(DepositMessages.AccountNotFound, isErrorMessage: true);
                    return View("Index", viewModel);
                }

                account.Balance += mAmount;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                AccountViewModel successViewModel = await _accountViewModel.WithMessage(DepositMessages.DepositSuccessful, isErrorMessage: false);
                return View("Index", successViewModel);
            }
            catch(Exception)
            {
                if(transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }

                AccountViewModel errorViewModel = await _accountViewModel.WithMessage(DepositMessages.DepositFailed, isErrorMessage: true);
                return View("Index", errorViewModel);
            }
        }
        #endregion
    }
}