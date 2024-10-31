namespace BankEase.Models
{
    public class ErrorViewModel
    {
        #region Properties
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
        #endregion
    }
}