using BankEase.Common;
using BankEase.Common.Messages;
using BankEase.Common.TransactionHelper;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankEase.Controllers
{
    public class WithdrawController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
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

            Account account = (await context.Accounts.FirstOrDefaultAsync(account => account.Id.Equals(nAccountId.Value)))!;

            AccountViewModel viewModel = new AccountViewModel(this.HttpContext, context) { CurrentSaldo = account.Balance };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(decimal mAmount)
        {
            if(mAmount <= 0m)
            {
                AccountViewModel viewModel = await _accountViewModel.WithMessage(WithdrawMessages.WithdrawAmountMustBeGreaterThanZero, isErrorMessage: true);
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
                    AccountViewModel viewModel = await _accountViewModel.WithMessage(WithdrawMessages.AccountNotFound, isErrorMessage: true);
                    return View("Index", viewModel);
                }

                if(account.Balance - mAmount < -account.Overdraft)
                {
                    AccountViewModel viewModel = await _accountViewModel.WithMessage(WithdrawMessages.WithdrawExceedsLimit, isErrorMessage: true);
                    return View("Index", viewModel);
                }

                context.TransactionRecords.Add(CreateTransactionRecord(account, mAmount));

                account.Balance -= mAmount;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                AccountViewModel successViewModel = await _accountViewModel.WithMessage(WithdrawMessages.WithdrawSuccessful, isErrorMessage: false);
                return View("Index", successViewModel);
            }
            catch(Exception)
            {
                if(transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }

                AccountViewModel errorViewModel = await _accountViewModel.WithMessage(WithdrawMessages.WithdrawFailed, isErrorMessage: true);
                return View("Index", errorViewModel);
            }
        }
        #endregion

        #region Privates
        private TransactionRecord CreateTransactionRecord(Account account, decimal amount)
        {
            return new TransactionRecord
                   {
                       AccountId = account.Id,
                       Amount = amount,
                       Type = TransactionType.Withdraw,
                       Text = TransactionType.WithdrawText,
                       TransactionTime = DateTime.Now,
                       Account = account
                   };
        }
        #endregion
    }
}