using BankEase.Common;
using BankEase.Common.Messages;
using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

public class TransactionController(DatabaseContext context, ValidationService validationService, SessionService sessionService, TransactionService transactionService, AccountService accountService) : Controller
{
    #region Fields
    public TransactionViewModel _transactionViewModel = new();
    #endregion

    #region Publics
    public async Task<IActionResult> Index()
    {
        if(!sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
            return RedirectToHomeOrAccount(nUserId);

        Account? account = await accountService.GetAccountById(nAccountId!.Value);
        if(account == null) return RedirectToAction("Index", "Account");

        _transactionViewModel.CurrentSaldo = account.Balance;
        return View(_transactionViewModel);
    }

    public async Task<IActionResult> Transfer(string strIBAN, decimal mAmount)
    {
        // IBAN- und Betragsvalidierung über den ValidationService
        if(!validationService.IsAmountValid(mAmount, out string? amountErrorMessage))
            return CreateErrorMessage(amountErrorMessage!, strIBAN, mAmount);

        if(!validationService.IsIBANValid(strIBAN, out string? ibanErrorMessage))
            return CreateErrorMessage(ibanErrorMessage!, strIBAN, mAmount);

        // Starte die Transaktion
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Validieren der UserSession
            if(!sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
                return RedirectToAction("Index", "Account");

            // Überprüfen ob ein Account vorhanden ist
            Account? account = await accountService.GetAccountById(nAccountId!.Value);
            if(account == null) return CreateErrorMessage(TransactionMessages.AccountNotFound, strIBAN, mAmount);

            // Den Account des Empfängers, anhand der IBAN laden
            Account? receivingAccount = await accountService.GetAccountByIBAN(strIBAN);
            if(receivingAccount == null)
                return CreateErrorMessage(TransactionMessages.NoMatchingAccountFoundToIBAN, strIBAN, mAmount);

            // Überprüfen ob ausreichend Guthaben auf dem Konto ist
            if(!transactionService.HasSufficientFunds(account, mAmount))
                return CreateErrorMessage(TransactionMessages.TransactionExceedsLimit, strIBAN, mAmount);

            // Transaktion erstellen und ausführen
            _transactionViewModel.CurrentSaldo = await transactionService.ExecuteTransactionAsync(account, receivingAccount, mAmount);
            await transaction.CommitAsync();

            return CreateSuccessMessage(TransactionMessages.TransferSuccessful);
        }
        catch(Exception)
        {
            await transaction.RollbackAsync();
            return CreateErrorMessage(TransactionMessages.TransferFailed, strIBAN, mAmount);
        }
    }
    #endregion

    #region Privates
    private IActionResult RedirectToHomeOrAccount(int? nUserId)
    {
        return nUserId is null or 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Index", "Account");
    }

    private IActionResult CreateErrorMessage(string message, string? iban = null, decimal? amount = null)
    {
        // Saldo des aktuellen Kontos abrufen, um ihn im Fehlerfall ebenfalls anzuzeigen
        int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);
        if(nAccountId.HasValue)
        {
            Account? account = context.Accounts.FirstOrDefault(a => a.Id == nAccountId.Value);
            if(account != null)
            {
                _transactionViewModel.CurrentSaldo = account.Balance;
            }
        }

        _transactionViewModel.ErrorMessage = message;
        _transactionViewModel.SuccessMessage = null;
        _transactionViewModel.IBAN = iban;
        _transactionViewModel.Amount = amount;

        return View("Index", _transactionViewModel);
    }

    private IActionResult CreateSuccessMessage(string message)
    {
        _transactionViewModel.SuccessMessage = message;
        _transactionViewModel.ErrorMessage = null;
        return View("Index", _transactionViewModel);
    }
    #endregion
}