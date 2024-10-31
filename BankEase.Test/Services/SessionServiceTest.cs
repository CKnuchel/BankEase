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
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private MockSession _mockSession = null!;
        private SessionService _sessionService = null!;
        #endregion

        #region Initialize and Cleanup
        [TestInitialize]
        public void TestInitialize()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mockSession = new MockSession();
            _sessionService = new SessionService(_httpContextAccessorMock.Object);

            // Mock HttpContext, sodass es eine MockSession zurückgibt
            Mock<HttpContext> httpContextMock = new();
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
            bool bResult = _sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId);

            // Assert
            Assert.IsTrue(bResult);
            Assert.AreEqual(1, nUserId);
            Assert.AreEqual(2, nAccountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenUserIdIsMissing()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 2);

            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId);

            // Assert
            Assert.IsFalse(bResult);
            Assert.IsNull(nUserId);
            Assert.AreEqual(2, nAccountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenAccountIdIsMissing()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 1);

            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId);

            // Assert
            Assert.IsFalse(bResult);
            Assert.AreEqual(1, nUserId);
            Assert.IsNull(nAccountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenUserIdAndAccountIdAreZeroOrNegative()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 0);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, -1);

            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId);

            // Assert
            Assert.IsFalse(bResult);
            Assert.AreEqual(0, nUserId);
            Assert.AreEqual(-1, nAccountId);
        }

        [TestMethod]
        public void IsAccountSessionValid_ReturnsTrue_WhenAccountIdIsPresentAndValid()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.USER_ID, 1);
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 1);

            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out _, out int? nAccountId);

            // Assert
            Assert.IsTrue(bResult);
            Assert.AreEqual(1, nAccountId);
        }

        [TestMethod]
        public void IsAccountSessionValid_ReturnsFalse_WhenAccountIdIsMissing()
        {
            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out _, out int? nAccountId);

            // Assert
            Assert.IsFalse(bResult);
            Assert.IsNull(nAccountId);
        }

        [TestMethod]
        public void IsAccountSessionValid_ReturnsFalse_WhenAccountIdIsZeroOrNegative()
        {
            // Arrange
            _mockSession.SetInt32(SessionKey.ACCOUNT_ID, 0);

            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out _, out int? nAccountId);

            // Assert
            Assert.IsFalse(bResult);
            Assert.AreEqual(0, nAccountId);
        }

        [TestMethod]
        public void IsUserSessionValid_ReturnsFalse_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(null as HttpContext);

            // Act
            bool bResult = _sessionService.IsAccountSessionValid(out int? nUserId, out int? nAccountId);

            // Assert
            Assert.IsFalse(bResult);
            Assert.IsNull(nUserId);
            Assert.IsNull(nAccountId);
        }
        #endregion
    }
}