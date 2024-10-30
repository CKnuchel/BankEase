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
        Account? account = await _transactionService.GetAccountById(1);

        // Assert
        Assert.IsNotNull(account);
        Assert.AreEqual(1, account.Id);
        Assert.AreEqual(VALID_SENDER_IBAN, account.IBAN);
    }

    [TestMethod]
    public async Task GetAccountById_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Act
        Account? account = await _transactionService.GetAccountById(999);

        // Assert
        Assert.IsNull(account);
    }

    [TestMethod]
    public async Task GetAccountByIBAN_ReturnsAccount_WhenIBANExists()
    {
        // Act
        Account? account = await _transactionService.GetAccountByIBAN(VALID_SENDER_IBAN);

        // Assert
        Assert.IsNotNull(account);
        Assert.AreEqual(VALID_SENDER_IBAN, account.IBAN);
    }

    [TestMethod]
    public async Task GetAccountByIBAN_ReturnsNull_WhenIBANDoesNotExist()
    {
        // Act
        Account? account = await _transactionService.GetAccountByIBAN("CH0000000000000000000");

        // Assert
        Assert.IsNull(account);
    }

    [TestMethod]
    public void HasSufficientFunds_ReturnsTrue_WhenBalanceIsSufficient()
    {
        // Arrange
        Account account = _inMemoryContext.Accounts.First();

        // Act
        bool result = _transactionService.HasSufficientFunds(account, 500m);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasSufficientFunds_ReturnsFalse_WhenBalanceIsInsufficient()
    {
        // Arrange
        Account account = _inMemoryContext.Accounts.First();

        // Act
        bool result = _transactionService.HasSufficientFunds(account, 2000m);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExecuteTransactionAsync_UpdatesBalancesAndReturnsUpdatedBalance()
    {
        // Arrange
        Account senderAccount = _inMemoryContext.Accounts.Include(account => account.Customer).First(a => a.IBAN == VALID_SENDER_IBAN);
        Account receiverAccount = new Account
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
        decimal updatedBalance = await _transactionService.ExecuteTransactionAsync(senderAccount, receiverAccount, 300m);

        // Assert
        Assert.AreEqual(700m, senderAccount.Balance);
        Assert.AreEqual(400m, receiverAccount.Balance);
        Assert.AreEqual(700m, updatedBalance);
    }
    #endregion

    #region Privates
    private void AddTestData()
    {
        Customer customer = new Customer
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

        Account account = new Account
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