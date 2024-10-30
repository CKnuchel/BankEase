namespace BankEase.ViewModel;

public class TransactionViewModel
{
    #region Properties
    public decimal CurrentSaldo { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? IBAN { get; set; }
    public decimal? Amount { get; set; }
    #endregion
}