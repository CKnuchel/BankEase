namespace BankEase.Common.TransactionHelper;

public static class TransactionType
{
    #region Properties
    public static char Deposit => 'C';
    public static string DepositText => "Einzahlung";
    public static char Withdraw => 'D';
    public static string WithdrawText => "Auszahlung";

    #endregion
}