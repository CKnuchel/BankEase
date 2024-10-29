using BankEase.Common;
using BankEase.Common.TransactionHelper;
using BankEase.Controllers;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

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
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                                                    .UseSqlite("DataSource=:memory:")
                                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                                    .Options;

        _inMemoryContext = new DatabaseContext(options);
        _inMemoryContext.Database.OpenConnection();
        _inMemoryContext.Database.EnsureCreated();

        _mockSession = new MockSession();

        Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();

        mockHttpContext.Setup(s => s.Session).Returns(_mockSession);
        mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

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
        const int userId = 1;
        const int accountId = 1;
        _mockSession.SetInt32(SessionKey.USER_ID, userId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, accountId);

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
        const int userId = 1;
        const int accountId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, userId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, accountId);

        decimal initialBalance = (await _inMemoryContext.Accounts.FindAsync(accountId))?.Balance ?? 0;

        ViewResult? result = await _controller.Deposit(100m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(DepositMessages.DepositSuccessful, viewModel.SuccessMessage);

        decimal? updatedBalance = (await _inMemoryContext.Accounts.FindAsync(accountId))?.Balance;
        Assert.AreEqual(initialBalance + 100m, updatedBalance);
    }

    [TestMethod]
    public async Task Deposit_CreatesTransactionRecord_OnSuccessfulDeposit()
    {
        const int userId = 1;
        const int accountId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, userId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, accountId);

        const decimal depositAmount = 100m;

        ViewResult? result = await _controller.Deposit(depositAmount) as ViewResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);

        TransactionRecord? transactionRecord = await _inMemoryContext.TransactionRecords
                                                                     .FirstOrDefaultAsync(t => t.AccountId == accountId && t.Amount == depositAmount && t.Type == TransactionType.Deposit);

        Assert.IsNotNull(transactionRecord);
        Assert.AreEqual(TransactionType.Deposit, transactionRecord.Type);
        Assert.AreEqual(TransactionType.DepositText, transactionRecord.Text);
        Assert.AreEqual(accountId, transactionRecord.AccountId);
        Assert.AreEqual(depositAmount, transactionRecord.Amount);
    }

    [TestMethod]
    public async Task Deposit_NoTransactionRecord_WhenTransactionFails()
    {
        const int userId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, userId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 999); // Ungültige Konto-ID

        ViewResult? result = await _controller.Deposit(50m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(DepositMessages.AccountNotFound, viewModel.ErrorMessage);

        bool transactionExists = await _inMemoryContext.TransactionRecords.AnyAsync();
        Assert.IsFalse(transactionExists, "Es sollten keine Transaktionsdatensätze existieren, wenn die Transaktion fehlschlägt.");
    }
    #endregion

    #region Privates
    private void AddTestData()
    {
        List<Customer> customers = new List<Customer>
                                   {
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
                                   };

        _inMemoryContext.Customers.AddRange(customers);

        _inMemoryContext.Accounts.AddRange(new List<Account>
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