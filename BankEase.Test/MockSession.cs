using Microsoft.AspNetCore.Http;

/// <inheritdoc />
internal class MockSession : ISession
{
    #region Fields
    private readonly Dictionary<string, byte[]?> _sessionStorage = new();
    #endregion

    #region Properties
    public string Id => "mock_session_id";
    public bool IsAvailable => true;
    public IEnumerable<string> Keys => _sessionStorage.Keys;
    #endregion

    #region Publics
    public void Clear()
    {
        _sessionStorage.Clear();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Remove(string strKey)
    {
        _sessionStorage.Remove(strKey);
    }

    public void Set(string strKey, byte[]? value)
    {
        _sessionStorage[strKey] = value;
    }

    public bool TryGetValue(string strKey, out byte[] value)
    {
        if(_sessionStorage.TryGetValue(strKey, out byte[]? objValue))
        {
            value = objValue!;
            return true;
        }

        value = null!;
        return false;
    }

    public void SetInt32(string strKey, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        _sessionStorage[strKey] = bytes;
    }

    public int? GetInt32(string strKey)
    {
        if(!_sessionStorage.TryGetValue(strKey, out byte[]? bytes) || bytes!.Length != 4) return null;

        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToInt32(bytes, 0);
    }
    #endregion
}