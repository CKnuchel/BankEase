using BankEase.Data;
using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Services;

public class AccountService(DatabaseContext context)
{
    #region Publics
    public async Task<List<Account>> GetAccountsByCustomerId(int nCustomerId)
    {
        return await context.Accounts
                            .Where(account => account.CustomerId == nCustomerId)
                            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerById(int nCustomerId)
    {
        return await context.Customers
                            .FirstOrDefaultAsync(customer => customer.Id == nCustomerId);
    }

    public async Task<Account?> GetAccountById(int nAccountId)
    {
        return await context.Accounts.FirstOrDefaultAsync(account => account.Id == nAccountId);
    }

    public async Task<Account?> GetAccountByIBAN(string strIBAN)
    {
        strIBAN = strIBAN.Replace(" ", "");
        return await context.Accounts.FirstOrDefaultAsync(account => account.IBAN == strIBAN);
    }
    #endregion
}