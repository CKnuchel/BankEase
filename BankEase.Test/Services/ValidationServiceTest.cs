using BankEase.Common.Messages;
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
        bool result = this._validationService.IsAmountValid(100m, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
    }

    [TestMethod]
    public void IsAmountValid_ReturnsFalse_WhenAmountIsZero()
    {
        // Act
        bool result = this._validationService.IsAmountValid(0m, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(TransactionMessages.TransferAmountMustBeGreaterThanZero, errorMessage);
    }

    [TestMethod]
    public void IsAmountValid_ReturnsFalse_WhenAmountIsNegative()
    {
        // Act
        bool result = this._validationService.IsAmountValid(-50m, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(TransactionMessages.TransferAmountMustBeGreaterThanZero, errorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsTrue_WhenIBANIsValid()
    {
        // Arrange
        string validIban = "CH 3200 5855 8512 3456 01T";

        // Act
        bool result = this._validationService.IsIBANValid(validIban, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsFalse_WhenIBANIsInvalidFormat()
    {
        // Arrange
        string invalidIban = "DE 3200 5855 8512 3456 01T"; // Invalid for CH regex

        // Act
        bool result = this._validationService.IsIBANValid(invalidIban, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(TransactionMessages.IBANInvalid, errorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsFalse_WhenIBANIsEmpty()
    {
        // Act
        bool result = this._validationService.IsIBANValid(string.Empty, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(TransactionMessages.IBANInvalid, errorMessage);
    }

    [TestMethod]
    public void IsIBANValid_ReturnsFalse_WhenIBANIsTooShort()
    {
        // Arrange
        string shortIban = "CH93 0076"; // Too short for valid IBAN format

        // Act
        bool result = this._validationService.IsIBANValid(shortIban, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(TransactionMessages.IBANInvalid, errorMessage);
    }
    #endregion
}