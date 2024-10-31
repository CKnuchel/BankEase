namespace BankEase.Common;

public static class WithdrawMessages
{
    #region Properties
    public static string WithdrawAmountMustBeGreaterThanZero => "Der eingegebene Betrag darf nicht im negativen Bereich sein.";
    public static string AccountNotFound => "Konto nicht gefunden.";
    public static string WithdrawSuccessful => "Der Betrag wurde erfolgreich von Ihrem Konto abgehoben.";
    public static string WithdrawExceedsLimit => "Der eingegebene Betrag übersteigt Ihr Limit.";
    public static string WithdrawFailed => "Der Betrag konnte nicht von Ihrem Konto abgehoben werden. Versuchen Sie es später erneut.";
    #endregion
}