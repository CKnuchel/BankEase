using Microsoft.AspNetCore.Http;

public class MockSession : ISession
{
    #region Fields
    private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();
    #endregion

    #region Properties
    public string Id { get; } = "mock_session_id";
    public bool IsAvailable { get; } = true;
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

    public void Remove(string key)
    {
        _sessionStorage.Remove(key);
    }

    public void Set(string key, byte[] value)
    {
        _sessionStorage[key] = value;
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        if(_sessionStorage.TryGetValue(key, out byte[]? objValue))
        {
            value = objValue;
            return true;
        }

        value = null!;
        return false;
    }

    // Speichert int-Werte unter Berücksichtigung der Endianness
    public void SetInt32(string key, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes); // Für konsistente Big-Endian-Speicherung
        }

        _sessionStorage[key] = bytes;
    }

    // Holt int-Werte und berücksichtigt die Endianness
    public int? GetInt32(string key)
    {
        if(_sessionStorage.TryGetValue(key, out byte[] bytes) && bytes.Length == 4)
        {
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Bytes wieder umkehren für richtige Interpretation
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        return null;
    }
    #endregion
}