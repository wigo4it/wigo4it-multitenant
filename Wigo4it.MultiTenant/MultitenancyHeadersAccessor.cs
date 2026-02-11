namespace Wigo4it.MultiTenant;

/// <summary>
/// Biedt toegang tot multi-tenant headers die zijn opgevangen uit HTTP-aanvragen of NServiceBus-berichten.
/// Hierdoor kan de HeaderForwarder headers doorsturen ongeacht hun bron.
/// Gebruikt AsyncLocal om headers per async-flow bij te houden.
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
    /// Bepaalt of een header key een door te sturen multi-tenant header is.
    /// </summary>
    public static bool IsForwardableHeader(string headerKey)
    {
        return headerKey.StartsWith("Wigo4it", StringComparison.OrdinalIgnoreCase)
            && headerKey.EndsWith("Forwardable", StringComparison.OrdinalIgnoreCase);
    }
}
