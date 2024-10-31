using BankEase.Common;
using BankEase.Services;

namespace BankEase.Test.Services;

[TestClass]
public class ValidationServiceTest
{
    #region Properties
    private ValidationService _validationService { get; set; } = null!;
    #endregion

    #region Initialize and Cleanup
    [TestInitialize]
    public void TestInitialize()
    {
        this._validationService = new ValidationService();
    }
    #endregion

    #region Tests
    [TestMethod]
    public void IsAmountValid_ReturnsTrue_WhenAmountIsGreaterThanZero()
    {
        // Act
        bool bResult = this._validationService.IsAmountValid(100m, out string? strErrorMessage);

        // Assert
        Assert.IsTrue(bResult);
        Assert.IsNull(strErrorMessage);
    }

    [TestMethod]
    public void IsAmountValid_ReturnsFalse_WhenAmountIsZero()
    {
        // Act
        bool bResult = this._validationService.IsAmountValid(0m, out string? strErrorMessage);

        // Assert
        Assert.IsFalse(bResult);
        Assert.AreEqual(TransactionMessages.TransferAmountMustBeGreaterThanZero, strErrorMessage);
    }

    [TestMethod]
    public void IsAmountValid_ReturnsFalse_WhenAmountIsNegative()
    {
        // Act
        bool bResult = this._validationService.IsAmountValid(-50m, out string? strErrorMessage);

        // Assert
        Assert.IsFalse(bResult);
        Assert.AreEqual(TransactionMessages.TransferAmountMustBeGreaterThanZero, strErrorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsTrue_WhenIBANIsValid()
    {
        // Arrange
        const string strValidIBAN = "CH 3200 5855 8512 3456 01T";

        // Act
        bool bResult = this._validationService.IsIBANValid(strValidIBAN, out string? strErrorMessage);

        // Assert
        Assert.IsTrue(bResult);
        Assert.IsNull(strErrorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsFalse_WhenIBANIsInvalidFormat()
    {
        // Arrange
        const string strInvalidIBAN = "DE 3200 5855 8512 3456 01T"; // Invalid da nicht CH Code

        // Act
        bool bResult = this._validationService.IsIBANValid(strInvalidIBAN, out string? strErrorMessage);

        // Assert
        Assert.IsFalse(bResult);
        Assert.AreEqual(TransactionMessages.IBANInvalid, strErrorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsFalse_WhenIBANIsEmpty()
    {
        // Act
        bool bResult = this._validationService.IsIBANValid(string.Empty, out string? strErrorMessage);

        // Assert
        Assert.IsFalse(bResult);
        Assert.AreEqual(TransactionMessages.IBANInvalid, strErrorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsFalse_WhenIBANIsTooShort()
    {
        // Arrange
        const string strShortIBAN = "CH93 0076"; // Too short for valid IBAN format

        // Act
        bool bResult = this._validationService.IsIBANValid(strShortIBAN, out string? strErrorMessage);

        // Assert
        Assert.IsFalse(bResult);
        Assert.AreEqual(TransactionMessages.IBANInvalid, strErrorMessage);
    }
    #endregion
}