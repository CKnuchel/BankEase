using BankEase.Common;
using BankEase.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BankEase.Test.Services
{
    [TestClass]
    public class SessionServiceTests
    {
        #region Fields
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private MockSession _mockSession;
        private SessionService _sessionService;
        #endregion

        #region Initialize and Cleanup
        [TestInitialize]
        public void TestInitialize()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mockSession = new MockSession();
            _sessionService = new SessionService(_httpContextAccessorMock.Object);

            // Mock HttpContext, sodass es eine MockSession zurückgibt
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(context => context.Session).Returns(_mockSession);
            _httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(httpContextMock.Object);
        }
        #endregion

        #region Tests
        [TestMethod]
        public void IsUserSessionValid_ReturnsTrue_WhenUserIdAndAccountIdArePresentAndValid()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 1);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 2);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? userId, out int? accountId);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, userId);
            Assert.AreEqual(2, accountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenUserIdIsMissing()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 2);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? userId, out int? accountId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(userId);
            Assert.AreEqual(2, accountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenAccountIdIsMissing()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 1);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? userId, out int? accountId);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, userId);
            Assert.IsNull(accountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenUserIdAndAccountIdAreZeroOrNegative()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 0);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, -1);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? userId, out int? accountId);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, userId);
            Assert.AreEqual(-1, accountId);
        }

        [TestMethod]
        public void IsAccountSessionValid_ReturnsTrue_WhenAccountIdIsPresentAndValid()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 1);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 1);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? _, out int? accountId);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, accountId);
        }

        [TestMethod]
        public void IsAccountSessionValid_ReturnsFalse_WhenAccountIdIsMissing()
        {
            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? _, out int? accountId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(accountId);
        }

        [TestMethod]
        public void IsAccountSessionValid_ReturnsFalse_WhenAccountIdIsZeroOrNegative()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 0);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? _, out int? accountId);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, accountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(null as HttpContext);

            // Act
            bool result = _sessionService.IsAccountSessionValid(out int? userId, out int? accountId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(userId);
            Assert.IsNull(accountId);
        }
        #endregion
    }
}