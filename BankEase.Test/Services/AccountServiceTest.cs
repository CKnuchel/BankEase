using BankEase.Data;
using BankEase.Models;
using BankEase.Services;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Test.Services;

[TestClass]
public class AccountServiceTests
{
    #region Fields
    private AccountService _accountService = null!;
    private DatabaseContext _inMemoryContext = null!;
    #endregion

    #region Initialize and Cleanup
    [TestInitialize]
    public void TestInitialize()
    {
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                                                    .UseInMemoryDatabase(databaseName: "BankEaseTestDb")
                                                    .Options;

        _inMemoryContext = new DatabaseContext(options);
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
    public async Task GetAccountsByCustomerId_ReturnsAccounts_WhenCustomerExists()
    {
        // Act
        List<Account> accounts = await _accountService.GetAccountsByCustomerId(1);

        // Assert
        Assert.IsNotNull(accounts);
        Assert.AreEqual(2, accounts.Count);
        Assert.AreEqual("CH9300762011623852957", accounts[0].IBAN);
        Assert.AreEqual("CH9300762011623852958", accounts[1].IBAN);
    }

    [TestMethod]
    public async Task GetAccountsByCustomerId_ReturnsEmptyList_WhenCustomerHasNoAccounts()
    {
        // Act
        List<Account> accounts = await _accountService.GetAccountsByCustomerId(2);

        // Assert
        Assert.IsNotNull(accounts);
        Assert.AreEqual(0, accounts.Count);
    }

    [TestMethod]
    public async Task GetCustomerById_ReturnsCustomer_WhenCustomerExists()
    {
        // Act
        Customer? customer = await _accountService.GetCustomerById(1);

        // Assert
        Assert.IsNotNull(customer);
        Assert.AreEqual(1, customer.Id);
        Assert.AreEqual("Max", customer.FirstName);
        Assert.AreEqual("Mustermann", customer.LastName);
    }

    [TestMethod]
    public async Task GetCustomerById_ReturnsNull_WhenCustomerDoesNotExist()
    {
        // Act
        Customer? customer = await _accountService.GetCustomerById(3);

        // Assert
        Assert.IsNull(customer);
    }
    #endregion

    #region Privates
    private void AddTestData()
    {
        Customer customer1 = new()
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

        Customer customer2 = new()
                             {
                                 Id = 2,
                                 FirstName = "Erika",
                                 LastName = "Musterfrau",
                                 City = "Musterstadt",
                                 CustomerNumber = "654321",
                                 Street = "Musterallee 2",
                                 Title = "Frau",
                                 ZipCode = 3000
                             };

        List<Account> accounts =
        [
            new() { Id = 1, CustomerId = 1, IBAN = "CH9300762011623852957", Balance = 1000m, Customer = customer1 },
            new() { Id = 2, CustomerId = 1, IBAN = "CH9300762011623852958", Balance = 2000m, Customer = customer1 }
        ];

        _inMemoryContext.Customers.AddRange(customer1, customer2);
        _inMemoryContext.Accounts.AddRange(accounts);
        _inMemoryContext.SaveChanges();
    }
    #endregion
}