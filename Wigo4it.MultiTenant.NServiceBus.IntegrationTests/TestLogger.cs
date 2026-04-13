using Microsoft.Extensions.Logging;

namespace Wigo4it.MultiTenant.NServiceBus.IntegrationTests;

/// <summary>
/// Aangepaste logger provider voor het vastleggen van logregels tijdens tests.
/// </summary>
internal class TestLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _logs = new();

    public IReadOnlyList<LogEntry> Logs => _logs.AsReadOnly();

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(categoryName, _logs);
    }

    public void Dispose() { }
}

/// <summary>
/// Aangepaste logger voor het vastleggen van afzonderlijke logregels.
/// </summary>
internal class TestLogger(string category, List<LogEntry> logs) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        logs.Add(new LogEntry { Category = category, Message = formatter(state, exception) });
    }
}

/// <summary>
/// Vertegenwoordigt een vastgelegde logregel.
/// </summary>
internal class LogEntry
{
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
