using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Test.Services;

[TestClass]
public class TransactionServiceTests
{
    #region Constants
    private const string VALID_SENDER_IBAN = "CH9300762011623852957";
    private const string VALID_RECEIVER_IBAN = "CH9300762011623852958";
    #endregion

    #region Fields
    private DatabaseContext _inMemoryContext = null!;
    private TransactionService _transactionService = null!;
    private AccountService _accountService = null!;
    #endregion

    #region Initialize and Cleanup
    [TestInitialize]
    public void TestInitialize()
    {
        // Verwende SQLite für die In-Memory-Datenbank
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                                                    .UseSqlite("DataSource=:memory:")
                                                    .Options;

        _inMemoryContext = new DatabaseContext(options);
        _inMemoryContext.Database.OpenConnection();
        _inMemoryContext.Database.EnsureCreated();

        _transactionService = new TransactionService(_inMemoryContext);
        _accountService = new AccountService(_inMemoryContext);
        AddTestData();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _inMemoryContext.Database.EnsureDeleted();
        _inMemoryContext.Dispose();
    }
    #endregion

    #region Tests
    [TestMethod]
    public async Task GetAccountById_ReturnsAccount_WhenAccountExists()
    {
        // Act
        Account? account = await _accountService.GetAccountById(1);

        // Assert
        Assert.IsNotNull(account);
        Assert.AreEqual(1, account.Id);
        Assert.AreEqual(VALID_SENDER_IBAN, account.IBAN);
    }

    [TestMethod]
    public async Task GetAccountById_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Act
        Account? account = await _accountService.GetAccountById(999);

        // Assert
        Assert.IsNull(account);
    }

    [TestMethod]
    public async Task GetAccountByIBAN_ReturnsAccount_WhenIBANExists()
    {
        // Act
        Account? account = await _accountService.GetAccountByIBAN(VALID_SENDER_IBAN);

        // Assert
        Assert.IsNotNull(account);
        Assert.AreEqual(VALID_SENDER_IBAN, account.IBAN);
    }

    [TestMethod]
    public async Task GetAccountByIBAN_ReturnsNull_WhenIBANDoesNotExist()
    {
        // Act
        Account? account = await _accountService.GetAccountByIBAN("CH0000000000000000000");

        // Assert
        Assert.IsNull(account);
    }

    [TestMethod]
    public void HasSufficientFunds_ReturnsTrue_WhenBalanceIsSufficient()
    {
        // Arrange
        Account account = _inMemoryContext.Accounts.First();

        // Act
        bool bResult = _transactionService.HasSufficientFunds(account, 500m);

        // Assert
        Assert.IsTrue(bResult);
    }

    [TestMethod]
    public void HasSufficientFunds_ReturnsFalse_WhenBalanceIsInsufficient()
    {
        // Arrange
        Account account = _inMemoryContext.Accounts.First();

        // Act
        bool bResult = _transactionService.HasSufficientFunds(account, 2000m);

        // Assert
        Assert.IsFalse(bResult);
    }

    [TestMethod]
    public async Task ExecuteTransactionAsync_UpdatesBalancesAndReturnsUpdatedBalance()
    {
        // Arrange
        Account senderAccount = _inMemoryContext.Accounts.Include(account => account.Customer).First(a => a.IBAN == VALID_SENDER_IBAN);
        Account receiverAccount = new()
                                  {
                                      Id = 2,
                                      CustomerId = 1,
                                      IBAN = VALID_RECEIVER_IBAN,
                                      Balance = 100,
                                      Customer = senderAccount.Customer,
                                      Overdraft = 0
                                  };

        _inMemoryContext.Accounts.Add(receiverAccount);
        await _inMemoryContext.SaveChangesAsync();

        // Act
        decimal mUpdatedBalance = await _transactionService.ExecuteTransactionAsync(senderAccount, receiverAccount, 300m);

        // Assert
        Assert.AreEqual(700m, senderAccount.Balance);
        Assert.AreEqual(400m, receiverAccount.Balance);
        Assert.AreEqual(700m, mUpdatedBalance);
    }

    [TestMethod]
    public async Task DepositAsync_IncreasesBalanceCorrectly()
    {
        // Arrange
        Account? account = await _accountService.GetAccountById(1);
        decimal mInitialBalance = account!.Balance;
        const decimal mDepositAmount = 200m;

        // Act
        decimal mUpdatedBalance = await _transactionService.DepositAsync(account, mDepositAmount);

        // Assert
        Assert.AreEqual(mInitialBalance + mDepositAmount, mUpdatedBalance);
        Assert.AreEqual(mUpdatedBalance, account.Balance);
    }

    [TestMethod]
    public async Task DepositAsync_CreatesTransactionRecord()
    {
        // Arrange
        Account? account = await _accountService.GetAccountById(1);
        int nInitialTransactionCount = _inMemoryContext.TransactionRecords.Count();
        const decimal mDepositAmount = 150m;

        // Act
        await _transactionService.DepositAsync(account!, mDepositAmount);
        int nFinalTransactionCount = _inMemoryContext.TransactionRecords.Count();

        // Assert
        Assert.AreEqual(nInitialTransactionCount + 1, nFinalTransactionCount);

        TransactionRecord transactionRecord = _inMemoryContext.TransactionRecords.OrderBy(tr => tr.TransactionTime).Last();
        Assert.IsNotNull(account);
        Assert.AreEqual(account.Id, transactionRecord.AccountId);
        Assert.AreEqual(mDepositAmount, transactionRecord.Amount);
        Assert.AreEqual(TransactionType.Deposit, transactionRecord.Type);
    }

    [TestMethod]
    public async Task WithdrawAsync_DecreasesBalanceCorrectly()
    {
        // Arrange
        Account? account = await _accountService.GetAccountById(1);
        decimal mInitialBalance = account!.Balance;
        const decimal mWithdrawAmount = 200m;

        // Act
        decimal mUpdatedBalance = await _transactionService.WithdrawAsync(account, mWithdrawAmount);

        // Assert
        Assert.AreEqual(mInitialBalance - mWithdrawAmount, mUpdatedBalance);
        Assert.AreEqual(mUpdatedBalance, account.Balance);
    }

    [TestMethod]
    public async Task WithdrawAsync_CreatesTransactionRecord()
    {
        // Arrange
        Account? account = await _accountService.GetAccountById(1);
        int nInitialTransactionCount = _inMemoryContext.TransactionRecords.Count();
        const decimal mWithdrawAmount = 150m;

        // Act
        await _transactionService.WithdrawAsync(account!, mWithdrawAmount);
        int nFinalTransactionCount = _inMemoryContext.TransactionRecords.Count();

        // Assert
        Assert.AreEqual(nInitialTransactionCount + 1, nFinalTransactionCount);

        TransactionRecord transactionRecord = _inMemoryContext.TransactionRecords.OrderBy(tr => tr.TransactionTime).Last();
        Assert.IsNotNull(account);
        Assert.AreEqual(account.Id, transactionRecord.AccountId);
        Assert.AreEqual(mWithdrawAmount, transactionRecord.Amount);
        Assert.AreEqual(TransactionType.Withdraw, transactionRecord.Type);
    }

    [TestMethod]
    public async Task WithdrawAsync_ReturnsUpdatedBalance_AfterTransaction()
    {
        // Arrange
        Account? account = await _accountService.GetAccountById(1);
        decimal mInitialBalance = account!.Balance;
        const decimal mWithdrawAmount = 100m;

        // Act
        decimal mUpdatedBalance = await _transactionService.WithdrawAsync(account, mWithdrawAmount);

        // Assert
        Assert.AreEqual(mInitialBalance - mWithdrawAmount, mUpdatedBalance);
    }
    #endregion

    #region Privates
    private void AddTestData()
    {
        Customer customer = new()
                            {
                                Id = 1,
                                FirstName = "Max",
                                LastName = "Mustermann",
                                City = "Musterstadt",
                                CustomerNumber = "123456",
                                Street = "Mustergasse 1",
                                Title = "Herr",
                                ZipCode = 3000
                            };

        Account account = new()
                          {
                              Id = 1,
                              CustomerId = 1,
                              IBAN = VALID_SENDER_IBAN,
                              Balance = 1000m,
                              Customer = customer,
                              Overdraft = 500m
                          };

        _inMemoryContext.Customers.Add(customer);
        _inMemoryContext.Accounts.Add(account);
        _inMemoryContext.SaveChanges();
    }
    #endregion
}