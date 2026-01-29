namespace Wigo4it.MultiTenant;

/// <summary>
/// Provides access to multi-tenancy headers captured from either HTTP requests or NServiceBus messages.
/// This allows the HeaderForwarder to forward headers regardless of their source.
/// Uses AsyncLocal to maintain headers per async flow.
/// </summary>
public class MultitenancyHeadersAccessor
{
    private static readonly AsyncLocal<Dictionary<string, string>> _asyncLocalHeaders = new();
    private static readonly IReadOnlyDictionary<string, string> _emptyHeaders = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> Headers => 
        _asyncLocalHeaders.Value ?? _emptyHeaders;

    public void SetHeader(string key, string value)
    {
        if (_asyncLocalHeaders.Value == null)
        {
            _asyncLocalHeaders.Value = new Dictionary<string, string>();
        }
        _asyncLocalHeaders.Value[key] = value;
    }

    public bool TryGetHeader(string key, out string? value)
    {
        if (_asyncLocalHeaders.Value != null)
        {
            return _asyncLocalHeaders.Value.TryGetValue(key, out value);
        }
        value = null;
        return false;
    }

    public void Clear()
    {
        _asyncLocalHeaders.Value?.Clear();
    }
    
    /// <summary>
    /// Determines if a header key is a forwardable multi-tenancy header.
    /// </summary>
    public static bool IsForwardableHeader(string headerKey)
    {
        return headerKey.StartsWith("Wigo4it", StringComparison.OrdinalIgnoreCase)
            && headerKey.EndsWith("Forwardable", StringComparison.OrdinalIgnoreCase);
    }
}
