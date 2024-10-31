using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.ViewModel;

public class AccountViewModel(HttpContext context, DatabaseContext databaseContext)
{
    #region Properties
    public decimal CurrentSaldo { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    #endregion

    #region Publics
    public async Task<AccountViewModel> WithMessage(string strMessage, bool bIsErrorMessage)
    {
        int? nUserId = context.Session.GetInt32(SessionKey.USER_ID);
        int? nAccountId = context.Session.GetInt32(SessionKey.ACCOUNT_ID);

        if(nUserId is null or 0) throw new ArgumentException(nameof(nUserId));
        if(nAccountId is null or 0) throw new ArgumentException(nameof(nAccountId));

        List<Account> userAccounts = await databaseContext.Accounts
                                                          .Where(account => account.Id == nAccountId)
                                                          .ToListAsync();

        return new AccountViewModel(context, databaseContext)
               {
                   CurrentSaldo = userAccounts.Sum(account => account.Balance),
                   ErrorMessage = bIsErrorMessage ? strMessage : null,
                   SuccessMessage = !bIsErrorMessage ? strMessage : null
               };
    }
    #endregion
}