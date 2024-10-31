namespace BankEase.Common;

public class TransactionMessages
{
    #region Properties
    public static string TransactionExceedsLimit => "Der eingegebene Betrag übersteigt Ihr Limit.";
    public static string AccountNotFound => "Konto nicht gefunden.";
    public static string TransferAmountMustBeGreaterThanZero => "Der eingegebene Betrag darf nicht im negativen Bereich sein.";
    public static string IBANInvalid => "Die eingegebene IBAN ist ungültig. Das zu verwendete Format ist: \nCH 1234 5678 9123 4567 8T";
    public static string NoMatchingAccountFoundToIBAN => "Es konnte kein Konto mit der eingegebenen IBAN gefunden werden.";
    public static string TransferSuccessful => "Die Transaktion wurde erfolgreich durchgeführt.";
    public static string TransferFailed => "Die Transaktion konnte aufgrund eines ubekannten Fehler nicht durchgeführt werden.";
    #endregion
}