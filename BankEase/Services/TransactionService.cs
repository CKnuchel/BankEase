using BankEase.Common.TransactionHelper;
using BankEase.Data;
using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Services;

public class TransactionService(DatabaseContext context)
{
    #region Publics
    public async Task<Account?> GetAccountById(int accountId)
    {
        return await context.Accounts.FirstOrDefaultAsync(account => account.Id == accountId);
    }

    public async Task<Account?> GetAccountByIBAN(string iban)
    {
        iban = iban.Replace(" ", "");
        return await context.Accounts.FirstOrDefaultAsync(account => account.IBAN == iban);
    }

    public bool HasSufficientFunds(Account account, decimal amount)
    {
        return account.Balance - amount >= -account.Overdraft;
    }

    public async Task<decimal> ExecuteTransactionAsync(Account account, Account receivingAccount, decimal amount)
    {
        // Transaktionsdatensätze erstellen und hinzufügen
        context.TransactionRecords.Add(CreateWithdrawTransactionRecord(account, amount));
        context.TransactionRecords.Add(CreateDepositTransactionRecord(receivingAccount, amount));

        // Guthaben aktualisieren
        account.Balance -= amount;
        receivingAccount.Balance += amount;

        // Speichern
        await context.SaveChangesAsync();

        // Rückgabe des aktualisierten Saldos
        return account.Balance;
    }

    public async Task<decimal> DepositAsync(Account account, decimal mAmount)
    {
        // Transaktionsdatensätze erstellen und hinzufügen
        context.TransactionRecords.Add(CreateDepositTransactionRecord(account, mAmount));

        // Guthaben aktualisieren
        account.Balance += mAmount;

        // Speichern 
        await context.SaveChangesAsync();

        // neues Guthaben zurückgeben
        return account.Balance;
    }
    #endregion

    #region Privates
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