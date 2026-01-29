using Microsoft.Extensions.Logging;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _logs = new();

    public IReadOnlyList<LogEntry> Logs => _logs;

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(categoryName, _logs);
    }

    public void Dispose()
    {
        _logs.Clear();
    }

    private class TestLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly List<LogEntry> _logs;

        public TestLogger(string categoryName, List<LogEntry> logs)
        {
            _categoryName = categoryName;
            _logs = logs;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            _logs.Add(new LogEntry
            {
                Category = _categoryName,
                LogLevel = logLevel,
                Message = message,
                Exception = exception
            });
        }
    }
}

public class LogEntry
{
    public string Category { get; set; } = string.Empty;
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}
