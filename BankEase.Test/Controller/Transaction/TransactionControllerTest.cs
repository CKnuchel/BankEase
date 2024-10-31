using BankEase.Common;
using BankEase.Data;
using BankEase.Models;
using BankEase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankEase.Test.Controller.Transaction
{
    [TestClass]
    public class TransactionControllerTest
    {
        #region Constants
        private const string SenderIban = "CH320058558512345602T";
        private const string ReceiverIban = "CH320058558512345601T";
        private const string InvalidIban = "InvalidIBAN";
        private const int SenderAccountId = 1;
        private const int ReceiverAccountId = 2;
        private const decimal InitialSenderBalance = 1000;
        private const decimal ReceiverBalance = 500;
        private const decimal OverdraftLimit = 200;
        private const int SenderUserId = 1;
        #endregion

        #region Fields
        private DatabaseContext _inMemoryContext = null!;
        private TransactionController _controller = null!;
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

            // MockSession erstellen
            _mockSession = new MockSession();

            // Setup von HttpContext und IHttpContextAccessor
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();

            // Sitzung in HttpContext einrichten
            mockHttpContext.Setup(s => s.Session).Returns(_mockSession);
            mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

            mockHttpContext.Setup(s => s.Session).Returns(_mockSession);
            mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(mockHttpContext.Object);

            // Controller initialisieren
            _controller = new TransactionController(_inMemoryContext, mockHttpContextAccessor.Object)
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
        public async Task Index_RedirectsToAccount_WhenSessionInvalid()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 0);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 0);

            // Act
            IActionResult result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.AreEqual("Index", ((RedirectToActionResult) result).ActionName);
            Assert.AreEqual("Home", ((RedirectToActionResult) result).ControllerName);
        }

        [TestMethod]
        public async Task Index_DisplaysCurrentSaldo_WhenSessionValid()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, SenderUserId);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, SenderAccountId);

            // Act
            ViewResult? result = await _controller.Index() as ViewResult;
            TransactionViewModel? viewModel = result?.Model as TransactionViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(InitialSenderBalance, viewModel!.CurrentSaldo);
        }

        [TestMethod]
        public async Task Transfer_DisplaysErrorMessage_WhenAmountInvalid()
        {
            // Arrange
            const decimal mInvalidAmount = -100;

            // Act
            IActionResult result = await _controller.Transfer(SenderIban, mInvalidAmount);
            ViewResult? viewResult = result as ViewResult;
            TransactionViewModel? viewModel = viewResult?.Model as TransactionViewModel;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
            Assert.AreEqual(TransactionMessages.TransferAmountMustBeGreaterThanZero, viewModel!.ErrorMessage);
        }

        [TestMethod]
        public async Task Transfer_DisplaysErrorMessage_WhenIBANInvalid()
        {
            // Arrange
            const decimal mValidAmount = 100;

            // Act
            IActionResult result = await _controller.Transfer(InvalidIban, mValidAmount);
            ViewResult? viewResult = result as ViewResult;
            TransactionViewModel? viewModel = viewResult?.Model as TransactionViewModel;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
            Assert.AreEqual(TransactionMessages.IBANInvalid, viewModel!.ErrorMessage);
        }

        [TestMethod]
        public async Task Transfer_SuccessfulTransfer_UpdatesSaldoAndReturnsSuccessMessage()
        {
            // Arrange
            const decimal mTransferAmount = 100;
            _mockSession.SetInt32(SessionKey.USER_ID, SenderUserId);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, SenderAccountId);

            // Act
            IActionResult result = await _controller.Transfer(ReceiverIban, mTransferAmount);
            ViewResult? viewResult = result as ViewResult;
            TransactionViewModel? viewModel = viewResult?.Model as TransactionViewModel;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
            Assert.AreEqual(TransactionMessages.TransferSuccessful, viewModel!.SuccessMessage);
            Assert.AreEqual(InitialSenderBalance - mTransferAmount, viewModel.CurrentSaldo);
        }

        [TestMethod]
        public async Task Transfer_AllowsTransferWithinOverdraftLimit()
        {
            // Arrange
            const decimal mTransferAmount = InitialSenderBalance + OverdraftLimit - 50; // 50 unterhalb des Overdraft Limits
            _mockSession.SetInt32(SessionKey.USER_ID, SenderUserId);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, SenderAccountId);

            // Act
            IActionResult result = await _controller.Transfer(ReceiverIban, mTransferAmount);
            ViewResult? viewResult = result as ViewResult;
            TransactionViewModel? viewModel = viewResult?.Model as TransactionViewModel;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
            Assert.AreEqual(TransactionMessages.TransferSuccessful, viewModel!.SuccessMessage);
            Assert.AreEqual(InitialSenderBalance - mTransferAmount, viewModel.CurrentSaldo);
        }

        [TestMethod]
        public async Task Transfer_DeniesTransferBeyondOverdraftLimit()
        {
            // Arrange
            const decimal mTransferAmount = InitialSenderBalance + OverdraftLimit + 50; // 50 über dem Overdraft Limit
            _mockSession.SetInt32(SessionKey.USER_ID, SenderUserId);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, SenderAccountId);

            // Act
            IActionResult result = await _controller.Transfer(ReceiverIban, mTransferAmount);
            ViewResult? viewResult = result as ViewResult;
            TransactionViewModel? viewModel = viewResult?.Model as TransactionViewModel;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);
            Assert.AreEqual(TransactionMessages.TransactionExceedsLimit, viewModel!.ErrorMessage);
        }
        #endregion

        #region Privates
        private void AddTestData()
        {
            Customer senderCustomer = new()
                                      {
                                          Id = SenderUserId,
                                          CustomerNumber = "123456",
                                          Title = "Herr",
                                          FirstName = "Max",
                                          LastName = "Mustermann",
                                          Street = "Mustergasse 1",
                                          City = "Musterstadt",
                                          ZipCode = 3000
                                      };

            Customer receiverCustomer = new()
                                        {
                                            Id = 2,
                                            CustomerNumber = "654321",
                                            Title = "Frau",
                                            FirstName = "Anna",
                                            LastName = "Beispiel",
                                            Street = "Beispielstrasse 2",
                                            City = "Beispielstadt",
                                            ZipCode = 4000
                                        };

            Models.Account senderAccount = new()
                                           {
                                               Id = SenderAccountId,
                                               CustomerId = senderCustomer.Id,
                                               IBAN = SenderIban,
                                               Balance = InitialSenderBalance,
                                               Customer = senderCustomer,
                                               Overdraft = OverdraftLimit
                                           };

            Models.Account receiverAccount = new()
                                             {
                                                 Id = ReceiverAccountId,
                                                 CustomerId = receiverCustomer.Id,
                                                 IBAN = ReceiverIban,
                                                 Balance = ReceiverBalance,
                                                 Customer = receiverCustomer,
                                                 Overdraft = 0
                                             };

            _inMemoryContext.Customers.Add(senderCustomer);
            _inMemoryContext.Customers.Add(receiverCustomer);
            _inMemoryContext.Accounts.Add(senderAccount);
            _inMemoryContext.Accounts.Add(receiverAccount);

            _inMemoryContext.SaveChanges();
        }
        #endregion
    }
}