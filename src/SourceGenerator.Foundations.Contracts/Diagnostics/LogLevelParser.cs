using System;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Utility for parsing log level strings from configuration
    /// </summary>
    public static class LogLevelParser
    {
        /// <summary>
        /// Parses a string to a LogLevel enum value
        /// </summary>
        /// <param name="value">The string value to parse</param>
        /// <param name="defaultLevel">The default level if parsing fails</param>
        /// <returns>The parsed log level or default if parsing fails</returns>
        public static LogLevel Parse(string? value, LogLevel defaultLevel = LogLevel.Information)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultLevel;

            if (Enum.TryParse<LogLevel>(value, ignoreCase: true, out var level))
                return level;

            // Try numeric parsing
            if (int.TryParse(value, out var numericLevel) && 
                Enum.IsDefined(typeof(LogLevel), numericLevel))
            {
                return (LogLevel)numericLevel;
            }

            return defaultLevel;
        }

        /// <summary>
        /// Tries to parse a string to a LogLevel enum value
        /// </summary>
        /// <param name="value">The string value to parse</param>
        /// <param name="level">The parsed log level</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool TryParse(string? value, out LogLevel level)
        {
            level = LogLevel.Information;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (Enum.TryParse<LogLevel>(value, ignoreCase: true, out level))
                return true;

            // Try numeric parsing
            if (int.TryParse(value, out var numericLevel) && 
                Enum.IsDefined(typeof(LogLevel), numericLevel))
            {
                level = (LogLevel)numericLevel;
                return true;
            }

            return false;
        }
    }
}
