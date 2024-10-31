using BankEase.Common;

namespace BankEase.Services;

public class SessionService(IHttpContextAccessor httpContextAccessor)
{
    #region Publics
    public bool IsUserSessionValid(out int? nUserId)
    {
        nUserId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey.USER_ID);
        return nUserId is > 0;
    }

    public bool IsAccountSessionValid(out int? nUserId, out int? nAccountId)
    {
        nUserId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey.USER_ID);
        nAccountId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey.ACCOUNT_ID);
        return nUserId is > 0 && nAccountId is > 0;
    }
    #endregion
}