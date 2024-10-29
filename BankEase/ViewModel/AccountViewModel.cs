using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.ViewModel;

public class AccountViewModel
{
    #region Fields
    private readonly HttpContext _context;
    private readonly DatabaseContext _databaseContext;
    #endregion

    #region Properties
    public decimal CurrentSaldo { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    #endregion

    #region Constructors
    public AccountViewModel(HttpContext context, DatabaseContext databaseContext)
    {
        _context = context;
        _databaseContext = databaseContext;
    }
    #endregion

    #region Publics
    public async Task<AccountViewModel> WithMessage(string message, bool isErrorMessage)
    {
        int? nUserId = _context.Session.GetInt32(SessionKey.USER_ID);
        int? nAccountId = _context.Session.GetInt32(SessionKey.ACCOUNT_ID);

        if(nUserId is null or 0) throw new ArgumentNullException(nameof(nUserId));
        if(nAccountId is null or 0) throw new ArgumentNullException(nameof(nAccountId));

        List<Account> userAccounts = await _databaseContext.Accounts
                                                           .Where(account => account.Id == nAccountId)
                                                           .ToListAsync();

        return new AccountViewModel(_context, _databaseContext)
               {
                   CurrentSaldo = userAccounts.Sum(account => account.Balance),
                   ErrorMessage = isErrorMessage ? message : null,
                   SuccessMessage = !isErrorMessage ? message : null
               };
    }
    #endregion
}