using BankEase.Common;
using BankEase.Common.Messages.AccountMessages;
using BankEase.Controllers;
using BankEase.Data;
using BankEase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankEase.Test.Controller.Account
{
    [TestClass]
    public class AccountControllerTest
    {
        #region Constants
        public const string IBAN_1 = "CH1234567890";
        public const string IBAN_2 = "CH0987654321";
        #endregion

        #region Fields
        private AccountController _controller = null!;
        private DatabaseContext _inMemoryContext = null!;
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

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            Mock<HttpContext> mockHttpContext = new();

            mockHttpContext.Setup(s => s.Session).Returns(_mockSession);

            mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

            _controller = new AccountController(_inMemoryContext, mockHttpContextAccessor.Object)
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
        public async Task Index_ReturnsViewWithAccountOptions_WhenUserIdIsProvided()
        {
            // Arrange
            const int userId = 1;
            const int accountId = 1;
            _mockSession.SetInt32(SessionKey.USER_ID, userId);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, accountId);

            // Act
            ViewResult? result = await _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(_controller.ViewBag.AccountOptions);

            List<SelectListItem>? accountOptions = _controller.ViewBag.AccountOptions as List<SelectListItem>;
            Assert.IsNotNull(accountOptions);
            Assert.AreEqual(2, accountOptions.Count);
            Assert.AreEqual(IBAN_1, accountOptions[0].Text);
            Assert.AreEqual(IBAN_2, accountOptions[1].Text);
        }

        [TestMethod]
        public void AccountSelection_RedirectsToDepositIndex_WhenAccountIdIsValid()
        {
            // Arrange
            const int nAccountId = 1;

            // Act
            RedirectToActionResult? result = _controller.AccountSelection(nAccountId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Deposit", result.ControllerName);
        }

        [TestMethod]
        public void AccountSelection_AddsModelErrorAndRedirectsToIndex_WhenAccountIdIsNull()
        {
            // Act
            RedirectToActionResult? result = _controller.AccountSelection(null) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.IsTrue(_controller.ModelState.ContainsKey("account"));
            Assert.AreEqual(AccountMessages.AccountNotSelected, _controller.ModelState["account"]?.Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void AccountSelection_AddsModelErrorAndRedirectsToIndex_WhenAccountIdIsZero()
        {
            // Act
            RedirectToActionResult? result = _controller.AccountSelection(0) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.IsTrue(_controller.ModelState.ContainsKey("account"));
            Assert.AreEqual(AccountMessages.AccountNotSelected, _controller.ModelState["account"]?.Errors[0].ErrorMessage);
        }
        #endregion

        #region Privates
        private void AddTestData()
        {
            List<Customer> customers = GetTestCustomers();

            _inMemoryContext.Customers.AddRange(customers);

            _inMemoryContext.Accounts.AddRange(new List<Models.Account>
                                               {
                                                   new()
                                                   {
                                                       Id = 1,
                                                       CustomerId = 1,
                                                       IBAN = IBAN_1,
                                                       Balance = 1000,
                                                       Customer = customers.First(),
                                                       Overdraft = 0
                                                   },
                                                   new()
                                                   {
                                                       Id = 2,
                                                       CustomerId = 1,
                                                       IBAN = IBAN_2,
                                                       Balance = 2000,
                                                       Customer = customers.First(),
                                                       Overdraft = 0
                                                   }
                                               });
            _inMemoryContext.SaveChanges();
        }

        private List<Customer> GetTestCustomers()
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
                },

                new()
                {
                    Id = 2,
                    FirstName = "Mina",
                    LastName = "Musterfrau",
                    City = "Musterstadt",
                    CustomerNumber = "654321",
                    Street = "Musterallee 2",
                    Title = "Frau",
                    ZipCode = 4000
                }
            ];

            return customers;
        }
        #endregion
    }
}