using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SGF.Sinks
{
    /// <summary>
    /// A instance of <see cref="ILogEventSink"/> that contains nested sinks
    /// that can be added and removed at runtime
    /// </summary>
    internal class LogEventSinkAggregate : ILogEventSink
    {
        private readonly List<ILogEventSink> m_sinks;

        public LogEventSinkAggregate()
        {
            m_sinks = new List<ILogEventSink>();
        }

        /// <summary>
        /// Adds a new sink
        /// </summary>
        public void Add(ILogEventSink sink)
        {
            m_sinks.Add(sink);
        }

        public void AddRange(IEnumerable<ILogEventSink>? sinks)
        {
            if(sinks == null)
            {
                return;
            }
            lock (m_sinks)
            {
                foreach (ILogEventSink sink in sinks)
                {
                    m_sinks.Add(sink);
                }
            }
        }

        /// <inheritdoc cref="ILogEventSink"/>
        public void Emit(LogEvent logEvent)
        {
            for (int i = 0; i < m_sinks.Count; i++)
            {
                ILogEventSink eventSink = m_sinks[i];
                eventSink.Emit(logEvent);
            }
        }
    }
}
