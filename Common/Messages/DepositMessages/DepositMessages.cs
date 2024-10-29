namespace BankEase.Common;

public static class DepositMessages
{
    public static string DepositAmountMustBeGreaterThanZero = "Der eingegebene Betrag darf nicht im negativen Bereich sein.";
    public static string AccountNotFound = "Konto nicht gefunden.";
    public static string DepositSuccessful = "Der Betrag wurde erfolgreich auf Ihrem Konto eingezahlt.";
    public static string DepositFailed = "Der Betrag konnte nicht auf Ihrem Konto eingezahlt werden. Versuchen Sie es später erneut.";
}