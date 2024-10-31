using BankEase.Common;

namespace BankEase.Services;

public class SessionService(IHttpContextAccessor httpContextAccessor)
{
    #region Publics
    public bool IsAccountSessionValid(out int? userId, out int? accountId)
    {
        userId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey.USER_ID);
        accountId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey.ACCOUNT_ID);
        return userId is > 0 && accountId is > 0;
    }

    public bool IsUserSessionValid(out int? nUserId)
    {
        nUserId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKey.USER_ID);
        return nUserId is > 0;
    }
    #endregion
}