using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankEase.Controllers
{
    public class DepositController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
    {
        #region Fields
        private readonly AccountViewModel _accountViewModel = new(httpContextAccessor.HttpContext!, context);
        private readonly SessionService _sessionService = new(httpContextAccessor);
        private readonly TransactionService _transactionService = new(context);
        private readonly ValidationService _validationService = new();
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            // Sitzungsvalidierung
            if(!_sessionService.IsAccountSessionValid(out int? userId, out int? accountId))
                return RedirectToHomeOrAccount(userId);

            // Kontodetails laden
            Account? account = await _transactionService.GetAccountById(accountId!.Value);
            if(account == null) return RedirectToAction("Index", "Account");

            AccountViewModel viewModel = new(this.HttpContext, context) { CurrentSaldo = account.Balance };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(decimal mAmount)
        {
            // Betragsvalidierung
            if(!_validationService.IsAmountValid(mAmount, out string? amountErrorMessage))
                return CreateErrorMessage(amountErrorMessage!);

            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Sitzungsvalidierung
                if(!_sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
                    return RedirectToAction("Index", "Account");

                Account? account = await _transactionService.GetAccountById(nAccountId!.Value);
                if(account == null)
                    return CreateErrorMessage(DepositMessages.AccountNotFound);

                // Transaktion ausführen
                decimal updatedBalance = await _transactionService.DepositAsync(account, mAmount);
                await transaction.CommitAsync();

                _accountViewModel.CurrentSaldo = updatedBalance;
                return CreateSuccessMessage(DepositMessages.DepositSuccessful);
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

        #region Privates
        private IActionResult RedirectToHomeOrAccount(int? userId)
        {
            return userId is null or 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Index", "Account");
        }

        private IActionResult CreateErrorMessage(string message)
        {
            _accountViewModel.ErrorMessage = message;
            _accountViewModel.SuccessMessage = null;
            return View("Index", _accountViewModel);
        }

        private IActionResult CreateSuccessMessage(string message)
        {
            _accountViewModel.SuccessMessage = message;
            _accountViewModel.ErrorMessage = null;
            return View("Index", _accountViewModel);
        }
        #endregion
    }
}