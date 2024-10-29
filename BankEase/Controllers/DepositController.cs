using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankEase.Controllers
{
    public class DepositController(DatabaseContext context) : Controller
    {
        #region Fields
        private readonly DatabaseContext _context = context;
        #endregion

        #region Publics
        public async Task<IActionResult> Index()
        {
            int? nUserId = this.HttpContext.Session.GetInt32(SessionKey.USER_ID);
            int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);

            if(nUserId is null or 0) return RedirectToAction("Index", "Home");
            if(nAccountId is null or 0) return RedirectToAction("Index", "Account");

            Account account = (await _context.Accounts.FirstOrDefaultAsync(account => account.Id.Equals(nAccountId.Value)))!;

            DepositViewModel viewModel = new DepositViewModel { CurrentSaldo = account.Balance };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(decimal mAmount)
        {
            // Validierung der Eingabe
            if(mAmount <= 0m)
            {
                DepositViewModel viewModel = await CreateDepositViewModelWithMessage(DepositMessages.DepositAmountMustBeGreaterThanZero, isErrorMessage: true);
                return View("Index", viewModel);
            }

            // Transaction starten
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);
                if(nAccountId is null or 0) return RedirectToAction("Index", "Account");

                Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == nAccountId);
                if(account == null)
                {
                    DepositViewModel viewModel = await CreateDepositViewModelWithMessage(DepositMessages.AccountNotFound, isErrorMessage: true);
                    return View("Index", viewModel);
                }

                account.Balance += mAmount;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                DepositViewModel successViewModel = await CreateDepositViewModelWithMessage(DepositMessages.DepositSuccessful, isErrorMessage: false);
                return View("Index", successViewModel);
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                DepositViewModel errorViewModel = await CreateDepositViewModelWithMessage(DepositMessages.DepositFailed, isErrorMessage: true);
                return View("Index", errorViewModel);
            }
        }
        #endregion

        #region Privates
        private async Task<DepositViewModel> CreateDepositViewModelWithMessage(string message, bool isErrorMessage)
        {
            int? nUserId = this.HttpContext.Session.GetInt32(SessionKey.USER_ID);
            int? nAccountId = this.HttpContext.Session.GetInt32(SessionKey.ACCOUNT_ID);

            if(nUserId is null or 0) throw new ArgumentNullException(nameof(nUserId));
            if(nAccountId is null or 0) throw new ArgumentNullException(nameof(nAccountId));

            List<Account> userAccounts = await _context.Accounts
                                                       .Where(account => account.Id == nAccountId)
                                                       .ToListAsync();

            return new DepositViewModel
                   {
                       CurrentSaldo = userAccounts.Sum(account => account.Balance),
                       ErrorMessage = isErrorMessage ? message : null,
                       SuccessMessage = !isErrorMessage ? message : null
                   };
        }
        #endregion
    }
}