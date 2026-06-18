using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Manages hierarchical log level configuration with support for
    /// global, generator-specific, and context/channel-specific levels
    /// </summary>
    public class LogLevelConfiguration
    {
        private readonly Dictionary<string, LogLevel> m_contextLevels;
        private LogLevel? m_generatorLevel;
        private LogLevel m_globalLevel;

        /// <summary>
        /// Gets or sets the global/default log level
        /// </summary>
        public LogLevel GlobalLevel
        {
            get => m_globalLevel;
            set => m_globalLevel = value;
        }

        /// <summary>
        /// Gets or sets the generator-specific log level override
        /// </summary>
        public LogLevel? GeneratorLevel
        {
            get => m_generatorLevel;
            set => m_generatorLevel = value;
        }

        public LogLevelConfiguration()
        {
            m_contextLevels = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase);
            m_globalLevel = LogLevel.Information; // Default
        }

        /// <summary>
        /// Sets the log level for a specific context/channel
        /// </summary>
        /// <param name="context">The context or channel name</param>
        /// <param name="level">The log level to set</param>
        public void SetContextLevel(string context, LogLevel level)
        {
            if (string.IsNullOrWhiteSpace(context))
                throw new ArgumentException("Context cannot be null or empty", nameof(context));

            m_contextLevels[context] = level;
        }

        /// <summary>
        /// Sets the context level for the given type
        /// </summary>
        public void SetContextLevel<T>(LogLevel level)
        {
            SetContextLevel(typeof(T).Name, level);
        }

        /// <summary>
        /// Gets the log level for a specific context/channel
        /// </summary>
        /// <param name="context">The context or channel name</param>
        /// <returns>The effective log level for this context</returns>
        public LogLevel GetContextLevel(string? context)
        {
            // Priority: Context-specific > Generator-specific > Global
            if (!string.IsNullOrWhiteSpace(context) && m_contextLevels.TryGetValue(context, out var contextLevel))
                return contextLevel;

            if (m_generatorLevel.HasValue)
                return m_generatorLevel.Value;

            return m_globalLevel;
        }

        /// <summary>
        /// Checks if a specific log level is enabled for a given context
        /// </summary>
        /// <param name="context">The context or channel name</param>
        /// <param name="level">The log level to check</param>
        /// <returns>True if the level is enabled</returns>
        public bool IsEnabled(string? context, LogLevel level)
        {
            var effectiveLevel = GetContextLevel(context);
            return effectiveLevel != LogLevel.None && level >= effectiveLevel;
        }

        /// <summary>
        /// Removes a context-specific log level override
        /// </summary>
        /// <param name="context">The context to remove</param>
        public bool RemoveContextLevel(string context)
        {
            return m_contextLevels.Remove(context);
        }

        /// <summary>
        /// Clears all context-specific log level overrides
        /// </summary>
        public void ClearContextLevels()
        {
            m_contextLevels.Clear();
        }
    }
}
