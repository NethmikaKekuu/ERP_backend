namespace AuthService.Services;

/// <summary>
/// Tracks the single active JTI (JWT ID) per user.
/// When a new token is issued via login, the old one is immediately revoked.
/// </summary>
public class TokenRevocationService
{
    // username → current active JTI
    private readonly Dictionary<string, string> _activeTokens = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    /// <summary>Call this after generating a new JWT. Replaces any previous active token for the user.</summary>
    public void SetActiveToken(string username, string jti)
    {
        lock (_lock)
        {
            _activeTokens[username] = jti;
        }
    }

    /// <summary>Returns true only if this JTI is the most recently issued token for the user.</summary>
    public bool IsTokenActive(string username, string jti)
    {
        lock (_lock)
        {
            return _activeTokens.TryGetValue(username, out var activeJti)
                   && activeJti == jti;
        }
    }
}
