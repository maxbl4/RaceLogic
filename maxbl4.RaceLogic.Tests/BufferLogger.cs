using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace maxbl4.RaceLogic.Tests
{
    public class BufferLogger<T> : BufferLogger, ILogger<T>
    {
    }

    public class BufferLogger : ILogger
    {
        public List<Message> Messages { get; } = new List<Message>(); 
        public IDisposable BeginScope<TState>(TState state)
        {
            return Disposable.Empty;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Messages.Add(new Message{LogLevel = logLevel, EventId = eventId, State = state, Exception = exception});
        }

        public class Message
        {
            public LogLevel LogLevel { get; set; }
            public EventId EventId { get; set; }
            public object State { get; set; }
            public Exception Exception { get; set; }
        }
    }
}