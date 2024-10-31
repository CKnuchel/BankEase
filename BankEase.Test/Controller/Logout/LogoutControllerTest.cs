using BankEase.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BankEase.Test.Controller.Logout;

[TestClass]
public class LogoutControllerTest
{
    #region Fields
    private LogoutController _controller = null!;
    private Mock<ISession> _sessionMock = null!;
    private Mock<HttpContext> _httpContextMock = null!;
    #endregion

    #region Initialize and Cleanup
    [TestInitialize]
    public void TestInitialize()
    {
        // Mock für die Sitzung
        _sessionMock = new Mock<ISession>();
        _httpContextMock = new Mock<HttpContext>();
        _httpContextMock.Setup(context => context.Session).Returns(_sessionMock.Object);

        // Controller instanziieren und HttpContext zuweisen
        _controller = new LogoutController
                      {
                          ControllerContext = new ControllerContext
                                              {
                                                  HttpContext = _httpContextMock.Object
                                              }
                      };
    }
    #endregion

    #region Tests
    [TestMethod]
    public void Logout_ClearsSessionAndRedirectsToHome()
    {
        // Act
        IActionResult result = _controller.Logout();

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

        //  Verifizieren, ob die Sitzung gelöscht wurde
        _sessionMock.Verify(session => session.Clear(), Times.Once);

        // Überprüfen, ob die Weiterleitung zur Startseite erfolgt
        RedirectToActionResult redirectResult = (RedirectToActionResult) result;
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("Home", redirectResult.ControllerName);
    }
    #endregion
}