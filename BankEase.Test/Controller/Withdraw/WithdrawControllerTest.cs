﻿using BankEase.Common;
using BankEase.Controllers;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankEase.Test.Controller.Withdraw;

[TestClass]
public class WithdrawControllerTest
{
    #region Fields
    private DatabaseContext _inMemoryContext = null!;
    private WithdrawController _controller = null!;
    private MockSession _mockSession = null!;
    #endregion

    #region Initialize and Cleanup
    [TestInitialize]
    public void TestInitialize()
    {
        // SQLite verwenden, da die InMemory-Datenbank keine Transaktionen unterstützt
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                                                    .UseSqlite("DataSource=:memory:") // SQLite InMemory-Datenbank
                                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                                    .Options;

        _inMemoryContext = new DatabaseContext(options);

        // Verbindung öffnen und Datenbank erstellen
        _inMemoryContext.Database.OpenConnection();
        _inMemoryContext.Database.EnsureCreated();

        _mockSession = new MockSession();

        // Setup von HttpContext und IHttpContextAccessor
        Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();

        // Sitzung in HttpContext einrichten
        mockHttpContext.Setup(s => s.Session).Returns(_mockSession);
        mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

        // Controller instanziieren
        _controller = new WithdrawController(_inMemoryContext, mockHttpContextAccessor.Object)
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
    public async Task Withdraw_DisplaysError_WhenAmountIsNegative()
    {
        // Arrange
        const int nUserId = 1;
        const int nAccountId = 1;
        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        // Act
        ViewResult? result = await _controller.Withdraw(-50m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(WithdrawMessages.WithdrawAmountMustBeGreaterThanZero, viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task Withdraw_DisplaysSuccessMessage_WhenAmountIsValid()
    {
        // Arrange
        const int nUserId = 1;
        const int nAccountId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        decimal mInitialBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance ?? 0;

        // Act
        ViewResult? result = await _controller.Withdraw(100m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(WithdrawMessages.WithdrawSuccessful, viewModel.SuccessMessage);

        // Überprüfe, ob der Kontostand korrekt aktualisiert wurde
        decimal? mUpdatedBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance;
        Assert.AreEqual(mInitialBalance - 100m, mUpdatedBalance);
    }

    [TestMethod]
    public async Task Withdraw_AllowsWithdrawal_UpToLimit()
    {
        // Arrange
        const int nUserId = 1;
        const int nAccountId = 1;
        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        decimal mInitialBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance ?? 0;
        Models.Account? account = await _inMemoryContext.Accounts.FindAsync(nAccountId);
        decimal mWithdrawAmount = mInitialBalance + account!.Overdraft;

        // Act
        ViewResult? result = await _controller.Withdraw(mWithdrawAmount) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(WithdrawMessages.WithdrawSuccessful, viewModel.SuccessMessage);

        decimal? mUpdatedBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance;
        Assert.AreEqual(-account.Overdraft, mUpdatedBalance);
    }

    [TestMethod]
    public async Task Withdraw_DisplaysError_WhenExceedingLimit()
    {
        // Arrange
        const int nUserId = 1;
        const int nAccountId = 1;
        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, nAccountId);

        decimal mInitialBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance ?? 0;
        Models.Account? account = await _inMemoryContext.Accounts.FindAsync(nAccountId);
        decimal mWithdrawAmount = mInitialBalance + account!.Overdraft + 0.05m; // Überschreiten der Limite

        // Act
        ViewResult? result = await _controller.Withdraw(mWithdrawAmount) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(WithdrawMessages.WithdrawExceedsLimit, viewModel.ErrorMessage);

        // Check if balance has not changed
        decimal? mUpdatedBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance;
        Assert.AreEqual(mInitialBalance, mUpdatedBalance);
    }

    [TestMethod]
    public async Task Withdraw_NoBalanceChange_WhenTransactionFails()
    {
        // Arrange
        const int nUserId = 1;
        const int nAccountId = 1;

        _mockSession.SetInt32(SessionKey.USER_ID, nUserId);
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 999); // Falsche Konto-ID, um Transaktionsfehler zu erzwingen

        decimal mInitialBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance ?? 0;

        // Act
        ViewResult? result = await _controller.Withdraw(50m) as ViewResult;
        AccountViewModel? viewModel = result?.Model as AccountViewModel;

        // Assert
        decimal? mUpdatedBalance = (await _inMemoryContext.Accounts.FindAsync(nAccountId))?.Balance;
        Assert.AreEqual(mInitialBalance, mUpdatedBalance);
    }

    [TestMethod]
    public async Task Withdraw_RedirectsToAccountIndex_WhenAccountIdIsInvalid()
    {
        // Arrange
        _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 0);

        // Act
        RedirectToActionResult? result = await _controller.Withdraw(50m) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("Account", result.ControllerName);
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