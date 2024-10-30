using System.Text.RegularExpressions;
using BankEase.Common;
using BankEase.Common.Messages;
using BankEase.Common.TransactionHelper;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class TransactionController(DatabaseContext context) : Controller
{
    #region Fields
    private readonly TransactionViewModel _transactionViewModel = new();
    private readonly Regex _ibanRegex = new(@"^[CH]{2}\s?(\d{4}\s?){4}\d{2}\s?[A-Z]");
    #endregion

    #region Publics
    public async Task<IActionResult> Index()
    {
        if(!IsUserSessionValid(out int? nUserId, out int? nAccountId))
            return RedirectToHomeOrAccount(nUserId);

        Account? account = await GetAccountById(nAccountId!.Value);
        if(account == null) return RedirectToAction("Index", "Account");

        _transactionViewModel.CurrentSaldo = account.Balance;
        return View(_transactionViewModel);
    }

    public async Task<IActionResult> Transfer(string strIBAN, decimal mAmount)
    {
        // Validierungen für Betrag und IBAN direkt anwenden
        if(!IsAmountValid(mAmount, strIBAN, mAmount) || !IsIBANValid(strIBAN, strIBAN, mAmount))
            return View("Index", _transactionViewModel);

        // Starte die Transaktion
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if(!IsAccountSessionValid(out int? nAccountId)) return RedirectToAction("Index", "Account");

            Account? account = await GetAccountById(nAccountId!.Value);
            if(account == null) return CreateErrorMessage(TransactionMessages.AccountNotFound, strIBAN, mAmount);

            Account? receivingAccount = await GetAccountByIBAN(strIBAN);
            if(receivingAccount == null)
                return CreateErrorMessage(TransactionMessages.NoMatchingAccountFoundToIBAN, strIBAN, mAmount);

            if(!HasSufficientFunds(account, mAmount))
                return CreateErrorMessage(TransactionMessages.TransactionExceedsLimit, strIBAN, mAmount);

            // Transaktion erstellen und ausführen
            await ExecuteTransaction(account, receivingAccount, mAmount);
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
    private bool IsUserSessionValid(out int? nUserId, out int? nAccountId)
    {
        nUserId = this.HttpContext.Session.GetInt32(SessionKey.USER_ID);
        nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);
        return nUserId is > 0 && nAccountId is > 0;
    }

    private bool IsAccountSessionValid(out int? nAccountId)
    {
        nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);
        return nAccountId is > 0;
    }

    private IActionResult RedirectToHomeOrAccount(int? nUserId)
    {
        return nUserId is null or 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Index", "Account");
    }

    private async Task<Account?> GetAccountById(int accountId)
    {
        return await context.Accounts.FirstOrDefaultAsync(account => account.Id == accountId);
    }

    private async Task<Account?> GetAccountByIBAN(string iban)
    {
        iban = iban.Replace(" ", "");
        return await context.Accounts.FirstOrDefaultAsync(account => account.IBAN == iban);
    }

    private bool IsAmountValid(decimal amount, string iban, decimal originalAmount)
    {
        if(amount > 0) return true;

        CreateErrorMessage(TransactionMessages.TransferAmountMustBeGreaterThanZero, iban, originalAmount);
        return false;
    }

    private bool IsIBANValid(string iban, string originalIBAN, decimal amount)
    {
        if(_ibanRegex.IsMatch(iban)) return true;

        CreateErrorMessage(TransactionMessages.IBANInvalid, originalIBAN, amount);
        return false;
    }

    private bool HasSufficientFunds(Account account, decimal amount)
    {
        return account.Balance - amount >= -account.Overdraft;
    }

    private async Task ExecuteTransaction(Account account, Account receivingAccount, decimal amount)
    {
        // Transaktionsdatensätze erstellen und hinzufügen
        context.TransactionRecords.Add(CreateWithdrawTransactionRecord(account, amount));
        context.TransactionRecords.Add(CreateDepositTransactionRecord(receivingAccount, amount));

        // Guthaben aktualisieren
        account.Balance -= amount;
        receivingAccount.Balance += amount;

        // Speichern
        await context.SaveChangesAsync();

        // Aktuellen Saldo nach dem Speichern abrufen und im ViewModel aktualisieren
        _transactionViewModel.CurrentSaldo = account.Balance;
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

    private TransactionRecord CreateWithdrawTransactionRecord(Account account, decimal amount)
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

    private TransactionRecord CreateDepositTransactionRecord(Account account, decimal amount)
    {
        return new TransactionRecord
               {
                   AccountId = account.Id,
                   Amount = amount,
                   Type = TransactionType.Deposit,
                   Text = TransactionType.DepositText,
                   TransactionTime = DateTime.Now,
                   Account = account
               };
    }
    #endregion
}