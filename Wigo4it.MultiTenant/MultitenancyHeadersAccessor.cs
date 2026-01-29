using System.Collections.Concurrent;

namespace Wigo4it.MultiTenant;

/// <summary>
/// Provides access to multi-tenancy headers captured from either HTTP requests or NServiceBus messages.
/// This allows the HeaderForwarder to forward headers regardless of their source.
/// Uses AsyncLocal to maintain headers per async flow.
/// </summary>
public class MultitenancyHeadersAccessor
{
    private static readonly AsyncLocal<ConcurrentDictionary<string, string>> _asyncLocalHeaders = new();

    public IReadOnlyDictionary<string, string> Headers => 
        _asyncLocalHeaders.Value ?? new ConcurrentDictionary<string, string>();

    public void SetHeader(string key, string value)
    {
        if (_asyncLocalHeaders.Value == null)
        {
            _asyncLocalHeaders.Value = new ConcurrentDictionary<string, string>();
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
}
