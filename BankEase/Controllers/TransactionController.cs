using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

public class TransactionController(DatabaseContext context, IHttpContextAccessor httpContextAccessor) : Controller
{
    #region Fields
    public TransactionViewModel _transactionViewModel = new();
    private readonly ValidationService _validationService = new();
    private readonly SessionService _sessionService = new(httpContextAccessor);
    private readonly AccountService _accountService = new(context);
    private readonly TransactionService _transactionService = new(context);
    #endregion

    #region Publics
    public async Task<IActionResult> Index()
    {
        if(!_sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId))
            return RedirectToHomeOrAccount(nUserId);

        if(_accountService.EnsureAccountBelongsToCustomer(nAccountId!.Value, nUserId!.Value).Result == false)
            return RedirectToHomeOrAccount(nUserId);

        Account? account = await _accountService.GetAccountById(nAccountId!.Value);
        if(account == null) return RedirectToAction("Index", "Account");

        _transactionViewModel.CurrentSaldo = account.Balance;
        return View(_transactionViewModel);
    }

    public async Task<IActionResult> Transfer(string strIBAN, decimal mAmount)
    {
        // IBAN- und Betragsvalidierung über den ValidationService
        if(!_validationService.IsAmountValid(mAmount, out string? strAmountErrorMessage))
            return CreateErrorMessage(strAmountErrorMessage!, strIBAN, mAmount);

        if(!_validationService.IsIBANValid(strIBAN, out string? strIBANErrorMessage))
            return CreateErrorMessage(strIBANErrorMessage!, strIBAN, mAmount);

        // Starte die Transaktion
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Validieren der UserSession
            if(!_sessionService.IsAccountSessionValid(out _, out int? nAccountId))
                return RedirectToAction("Index", "Account");

            // Überprüfen ob ein Account vorhanden ist
            Account? account = await _accountService.GetAccountById(nAccountId!.Value);
            if(account == null)
                return CreateErrorMessage(TransactionMessages.AccountNotFound, strIBAN, mAmount);

            // Den Account des Empfängers, anhand der IBAN laden
            Account? receivingAccount = await _accountService.GetAccountByIBAN(strIBAN);
            if(receivingAccount == null)
                return CreateErrorMessage(TransactionMessages.NoMatchingAccountFoundToIBAN, strIBAN, mAmount);

            // Überprüfen ob ausreichend Guthaben auf dem Konto ist
            if(!_transactionService.HasSufficientFunds(account, mAmount))
                return CreateErrorMessage(TransactionMessages.TransactionExceedsLimit, strIBAN, mAmount);

            // Transaktion erstellen und ausführen
            _transactionViewModel.CurrentSaldo = await _transactionService.ExecuteTransactionAsync(account, receivingAccount, mAmount);
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
    private IActionResult CreateSuccessMessage(string strMessage)
    {
        _transactionViewModel.SuccessMessage = strMessage;
        _transactionViewModel.ErrorMessage = null;
        return View("Index", _transactionViewModel);
    }
    #endregion

    private protected IActionResult RedirectToHomeOrAccount(int? nUserId)
    {
        return nUserId is null or 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Index", "Account");
    }

    private protected IActionResult CreateErrorMessage(string strMessage, string? strIBAN = null, decimal? mAmount = null)
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

        _transactionViewModel.ErrorMessage = strMessage;
        _transactionViewModel.SuccessMessage = null;
        _transactionViewModel.IBAN = strIBAN;
        _transactionViewModel.Amount = mAmount;

        return View("Index", _transactionViewModel);
    }
}