using System.Text.RegularExpressions;
using BankEase.Common;

namespace BankEase.Services;

public class ValidationService
{
    #region Fields
    private readonly Regex _ibanRegex = new(@"^[CH]{2}\s?(\d{4}\s?){4}\d{2}\s?[A-Z]", RegexOptions.NonBacktracking);
    #endregion

    #region Publics
    public bool IsAmountValid(decimal mAmount, out string? strErrorMessage)
    {
        if(mAmount > 0)
        {
            strErrorMessage = null;
            return true;
        }

        strErrorMessage = TransactionMessages.TransferAmountMustBeGreaterThanZero;
        return false;
    }

    public bool IsIBANValid(string strIBAN, out string? strErrorMessage)
    {
        if(_ibanRegex.IsMatch(strIBAN))
        {
            strErrorMessage = null;
            return true;
        }

        strErrorMessage = TransactionMessages.IBANInvalid;
        return false;
    }
    #endregion
}