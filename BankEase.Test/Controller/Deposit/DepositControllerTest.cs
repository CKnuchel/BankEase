using BankEase.Common;
using BankEase.Controllers;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankEase.Test.Controller.Deposit;

[TestClass]
public class DepositControllerTest
{
    #region Fields
    private DatabaseContext _inMemoryContext = null!;
    private DepositController _controller = null!;
    private MockSession _mockSession = null!;
    #endregion

    #region Initialize and Cleanup
    [TestInitialize]
    public void TestInitialize()
    {
        // SQLite verwenden, da die InMemory-Datenbank keine Transaktionen unterstützt
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                                                    .UseSqlite("DataSource=:memory:")
                                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                                    .Options;

        _inMemoryContext = new DatabaseContext(options);
        _inMemoryContext.Database.OpenConnection();
        _inMemoryContext.Database.EnsureCreated();

        _mockSession = new MockSession();

        // Mocken des IHttpContextAccessor
        Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
        Mock<HttpContext> mockHttpContext = new();

        // Mocken des HttpContext
        mockHttpContext.Setup(s => s.Session).Returns(_mockSession);

        // Mocken des HttpContextAccessor
        mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

        // Controller initialisieren
        _controller = new DepositController(_inMemoryContext, mockHttpContextAccessor.Object)
                      {
                          ControllerContext = new ControllerContext
                                              {
                                                  HttpContext = mockHttpContext.Object
                                              }
                      };

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
    public async Task Deposit_DisplaysError_WhenAmountIsNegative()
    {
        const int nUserId = 1;
        const int nAccountId = 1;
        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        ViewResult? result = await _controller.Deposit(-50m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(DepositMessages.DepositAmountMustBeGreaterThanZero, viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task Deposit_DisplaysSuccessMessage_WhenAmountIsValid()
    {
        const int nUserId = 1;
        const int nAccountId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        decimal mInitialBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance ?? 0;

        ViewResult? result = await _controller.Deposit(100m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(DepositMessages.DepositSuccessful, viewModel.SuccessMessage);

        decimal? updatedBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance;
        Assert.AreEqual(mInitialBalance + 100m, updatedBalance);
    }

    [TestMethod]
    public async Task Deposit_CreatesTransactionRecord_OnSuccessfulDeposit()
    {
        const int nUserId = 1;
        const int nAccountId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        const decimal mDepositAmount = 100m;

        ViewResult? result = await _controller.Deposit(mDepositAmount) as ViewResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);

        TransactionRecord? transactionRecord = await _inMemoryContext.TransactionRecords
                                                                     .FirstOrDefaultAsync(t => t.AccountId == nAccountId && t.Amount == mDepositAmount && t.Type == TransactionType.Deposit);

        Assert.IsNotNull(transactionRecord);
        Assert.AreEqual(TransactionType.Deposit, transactionRecord.Type);
        Assert.AreEqual(TransactionType.DepositText, transactionRecord.Text);
        Assert.AreEqual(nAccountId, transactionRecord.AccountId);
        Assert.AreEqual(mDepositAmount, transactionRecord.Amount);
    }

    [TestMethod]
    public async Task Deposit_NoTransactionRecord_WhenTransactionFails()
    {
        const int nUserId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 999); // Ungültige Konto-ID

        ViewResult? result = await _controller.Deposit(50m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(DepositMessages.AccountNotFound, viewModel.ErrorMessage);

        bool bTransactionExists = await _inMemoryContext.TransactionRecords.AnyAsync();
        Assert.IsFalse(bTransactionExists, "Es sollten keine Transaktionsdatensätze existieren, wenn die Transaktion fehlschlägt.");
    }
    #endregion

    #region Privates
    private void AddTestData()
    {
        List<Customer> customers =
        [
            new()
            {
                Id = 1,
                FirstName = "Max",
                LastName = "Mustermann",
                City = "Musterstadt",
                CustomerNumber = "123456",
                Street = "Mustergasse 1",
                Title = "Herr",
                ZipCode = 3000
            }
        ];

        _inMemoryContext.Customers.AddRange(customers);

        _inMemoryContext.Accounts.AddRange(new List<Models.Account>
                                           {
                                               new()
                                               {
                                                   Id = 1,
                                                   CustomerId = 1,
                                                   IBAN = "CH1234567890",
                                                   Balance = 1000,
                                                   Customer = customers.First(),
                                                   Overdraft = 0
                                               }
                                           });

        _inMemoryContext.SaveChanges();
    }
    #endregion
}