using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankEase.Controllers
{
    public class WithdrawController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
    {
        #region Fields
        private readonly AccountViewModel _accountViewModel = new();
        private readonly SessionService _sessionService = new(httpContextAccessor);
        private readonly TransactionService _transactionService = new(context);
        private readonly ValidationService _validationService = new();
        private readonly AccountService _accountService = new(context);
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            if(!_sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
                return RedirectToHomeOrAccount(nUserId);

            Account? account = await _accountService.GetAccountById(nAccountId!.Value);
            if(account == null) return RedirectToAction("Index", "Account");

            _accountViewModel.CurrentSaldo = account.Balance;
            return View(_accountViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(decimal mAmount)
        {
            if(!_validationService.IsAmountValid(mAmount, out string? strAmountErrorMessage))
                return CreateErrorMessage(strAmountErrorMessage!);

            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                if(!_sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
                    return RedirectToAction("Index", "Account");

                if(_accountService.EnsureAccountBelongsToCustomer(nAccountId!.Value, nUserId!.Value).Result == false)
                    return RedirectToAction("Index", "Home");

                Account? account = await _accountService.GetAccountById(nAccountId!.Value);
                if(account == null)
                    return CreateErrorMessage(WithdrawMessages.AccountNotFound);

                if(!_transactionService.HasSufficientFunds(account, mAmount))
                    return CreateErrorMessage(WithdrawMessages.WithdrawExceedsLimit);

                decimal updatedBalance = await _transactionService.WithdrawAsync(account, mAmount);
                await transaction.CommitAsync();

                _accountViewModel.CurrentSaldo = updatedBalance;
                return CreateSuccessMessage(WithdrawMessages.WithdrawSuccessful);
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                return CreateErrorMessage(WithdrawMessages.WithdrawFailed);
            }
        }
        #endregion

        private protected IActionResult CreateErrorMessage(string strErrorMessage)
        {
            if(_sessionService.IsAccountSessionValid(out _, out int? nAccountId))
            {
                Account? account = _accountService.GetAccountById(nAccountId!.Value).Result;
                _accountViewModel.CurrentSaldo = account?.Balance ?? 0;
            }

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

        private protected IActionResult RedirectToHomeOrAccount(int? nUserId)
        {
            return nUserId is null or 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Index", "Account");
        }
    }
}