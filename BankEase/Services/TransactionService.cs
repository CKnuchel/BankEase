using BankEase.Common;
using BankEase.Data;
using BankEase.Models;

namespace BankEase.Services;

public class TransactionService(DatabaseContext context)
{
    #region Publics
    public bool HasSufficientFunds(Account account, decimal mAmount)
    {
        return account.Balance - mAmount >= -account.Overdraft;
    }

    public async Task<decimal> ExecuteTransactionAsync(Account account, Account receivingAccount, decimal mAmount)
    {
        // Transaktionsdatensätze erstellen und hinzufügen
        context.TransactionRecords.Add(CreateWithdrawTransactionRecord(account, mAmount));
        context.TransactionRecords.Add(CreateDepositTransactionRecord(receivingAccount, mAmount));

        // Guthaben aktualisieren
        account.Balance -= mAmount;
        receivingAccount.Balance += mAmount;

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

    public async Task<decimal> WithdrawAsync(Account account, decimal mAmount)
    {
        // Transaktionsdatensätze erstellen und hinzufügen
        context.TransactionRecords.Add(CreateWithdrawTransactionRecord(account, mAmount));

        // Guthaben aktualisieren
        account.Balance -= mAmount;

        // Speichern 
        await context.SaveChangesAsync();

        // neues Guthaben zurückgeben
        return account.Balance;
    }
    #endregion

    #region Privates
    private static TransactionRecord CreateWithdrawTransactionRecord(Account account, decimal mAmount)
    {
        return new TransactionRecord
               {
                   AccountId = account.Id,
                   Amount = mAmount,
                   Type = TransactionType.Withdraw,
                   Text = TransactionType.WithdrawText,
                   TransactionTime = DateTime.Now,
                   Account = account
               };
    }

    private static TransactionRecord CreateDepositTransactionRecord(Account account, decimal mAmount)
    {
        return new TransactionRecord
               {
                   AccountId = account.Id,
                   Amount = mAmount,
                   Type = TransactionType.Deposit,
                   Text = TransactionType.DepositText,
                   TransactionTime = DateTime.Now,
                   Account = account
               };
    }
    #endregion
}