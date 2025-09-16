using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppSms.SimpleLogger
{
    public class SimpleFileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new();

        public SimpleFileLogger(string filePath) => _filePath = filePath;

        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}";
            lock (_lock)
            {
                File.AppendAllText(_filePath, message + Environment.NewLine);
            }
        }
    }

    public class SimpleFileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        public SimpleFileLoggerProvider(string filePath) => _filePath = filePath;
        public ILogger CreateLogger(string categoryName) => new SimpleFileLogger(_filePath);
        public void Dispose() { }
    }
}
