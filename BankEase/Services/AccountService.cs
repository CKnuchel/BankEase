using BankEase.Data;
using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Services;

public class AccountService(DatabaseContext context)
{
    #region Publics
    public async Task<List<Account>> GetAccountsByCustomerId(int customerId)
    {
        return await context.Accounts
                            .Where(account => account.CustomerId == customerId)
                            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerById(int customerId)
    {
        return await context.Customers
                            .FirstOrDefaultAsync(customer => customer.Id == customerId);
    }

    public async Task<Account?> GetAccountById(int accountId)
    {
        return await context.Accounts.FirstOrDefaultAsync(account => account.Id == accountId);
    }

    public async Task<Account?> GetAccountByIBAN(string iban)
    {
        iban = iban.Replace(" ", "");
        return await context.Accounts.FirstOrDefaultAsync(account => account.IBAN == iban);
    }
    #endregion
}