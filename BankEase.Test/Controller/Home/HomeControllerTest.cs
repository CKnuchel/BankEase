using BankEase.Common;
using BankEase.Controllers;
using BankEase.Data;
using BankEase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BankEase.Test.Controller.Home
{
    [TestClass]
    public class HomeControllerTest
    {
        #region Fields
        private DatabaseContext _inMemoryContext = null!;
        private MockSession _mockSession = null!;
        private HomeController _controller = null!;
        #endregion

        #region Initialize and Cleanup
        [TestInitialize]
        public void TestInitialize()
        {
            // In-Memory DatabaseContext erstellen
            DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                                                        .UseInMemoryDatabase(databaseName: "TestDatabase")
                                                        .Options;
            _inMemoryContext = new DatabaseContext(options);

            // MockSession erstellen
            _mockSession = new MockSession();

            // HttpContext-Setup für den Controller
            Mock<HttpContext> mockHttpContext = new();
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();

            mockHttpContext.Setup(s => s.Session).Returns(_mockSession);
            mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

            // Controller initialisieren
            _controller = new HomeController(_inMemoryContext)
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
        public async Task Index_ReturnsViewWithCustomers()
        {
            // Act
            ViewResult? result = await _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsNotNull(_controller.ViewBag.CustomerOptions);
            Assert.IsInstanceOfType(_controller.ViewBag.CustomerOptions, typeof(List<SelectListItem>));

            List<SelectListItem>? customerOptions = _controller.ViewBag.CustomerOptions as List<SelectListItem>;
            Assert.IsNotNull(customerOptions);
            Assert.AreEqual(2, customerOptions.Count);
            Assert.AreEqual("Max Mustermann", customerOptions[0].Text);
            Assert.AreEqual("Mina Musterfrau", customerOptions[1].Text);
        }

        [TestMethod]
        public void Login_RedirectsToAccountIndex_WhenUserIsValid()
        {
            // Arrange
            const int nValidUserId = 1;
            _mockSession.SetInt32(SessionKey.USER_ID, nValidUserId);

            // Act
            RedirectToActionResult? result = _controller.Login(nValidUserId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Account", result.ControllerName);

            // Überprüfe, ob die Session gesetzt wurde
            int? sessionUserId = _mockSession.GetInt32(SessionKey.USER_ID);
            Assert.IsNotNull(sessionUserId);
            Assert.AreEqual(nValidUserId, sessionUserId);
        }

        [TestMethod]
        public void Login_AddsModelErrorAndRedirectsToIndex_WhenUserIsNull()
        {
            // Act
            RedirectToActionResult? result = _controller.Login(null) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.IsTrue(_controller.ModelState.ContainsKey("user"));
        }

        [TestMethod]
        public void Login_AddsModelErrorAndRedirectsToIndex_WhenUserIsZero()
        {
            // Act
            RedirectToActionResult? result = _controller.Login(0) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.IsTrue(_controller.ModelState.ContainsKey("user"));
        }
        #endregion

        #region Privates
        private void AddTestData()
        {
            _inMemoryContext.Customers.AddRange(new List<Customer>
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
                                                });
            _inMemoryContext.SaveChanges();
        }
        #endregion
    }
}