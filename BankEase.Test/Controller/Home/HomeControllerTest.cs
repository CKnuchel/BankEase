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
        private DatabaseContext _inMemoryContext;
        private Mock<ISession> _mockSession;
        private HomeController _controller;
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

            // Session mocken
            _mockSession = new Mock<ISession>();

            // HttpContext-Setup für den Controller
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(s => s.Session).Returns(_mockSession.Object);

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