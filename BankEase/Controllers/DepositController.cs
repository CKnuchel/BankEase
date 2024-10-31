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
        private readonly AccountService _accountService = new(context);
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            // Sitzungsvalidierung
            if(!_sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
                return RedirectToHomeOrAccount(nUserId);

            // Kontodetails laden
            Account? account = await _accountService.GetAccountById(nAccountId!.Value);
            if(account == null) return RedirectToAction("Index", "Account");

            AccountViewModel viewModel = new(this.HttpContext, context) { CurrentSaldo = account.Balance };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(decimal mAmount)
        {
            // Betragsvalidierung
            if(!_validationService.IsAmountValid(mAmount, out string? strAmountErrorMessage))
                return CreateErrorMessage(strAmountErrorMessage!);

            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Sitzungsvalidierung
                if(!_sessionService.IsAccountSessionValid(out _, out int? nAccountId))
                    return RedirectToAction("Index", "Account");

                Account? account = await _accountService.GetAccountById(nAccountId!.Value);
                if(account == null) return CreateErrorMessage(DepositMessages.AccountNotFound);

                // Transaktion ausführen
                decimal mUpdatedBalance = await _transactionService.DepositAsync(account, mAmount);
                await transaction.CommitAsync();

                _accountViewModel.CurrentSaldo = mUpdatedBalance;
                return CreateSuccessMessage(DepositMessages.DepositSuccessful);
            }
            catch(Exception)
            {
                if(transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }

                AccountViewModel errorViewModel = await _accountViewModel.WithMessage(DepositMessages.DepositFailed, bIsErrorMessage: true);
                return View("Index", errorViewModel);
            }
        }
        #endregion

        private protected IActionResult RedirectToHomeOrAccount(int? nUserId)
        {
            return nUserId is null or 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Index", "Account");
        }

        private protected IActionResult CreateErrorMessage(string strErrorMessage)
        {
            _accountViewModel.ErrorMessage = strErrorMessage;
            _accountViewModel.SuccessMessage = null;
            return View("Index", _accountViewModel);
        }

        private protected IActionResult CreateSuccessMessage(string strSuccessMessage)
        {
            _accountViewModel.SuccessMessage = strSuccessMessage;
            _accountViewModel.ErrorMessage = null;
            return View("Index", _accountViewModel);
        }
    }
}