namespace BankEase.ViewModel;

public class AccountViewModel
{
    #region Properties
    public decimal CurrentSaldo { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    #endregion
}