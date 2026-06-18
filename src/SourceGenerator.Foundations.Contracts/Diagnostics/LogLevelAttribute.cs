using System;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Specifies the default log level for a source generator
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class LogLevelAttribute : Attribute
    {
        /// <summary>
        /// Gets the minimum log level
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogLevelAttribute"/> class
        /// </summary>
        /// <param name="level">The minimum log level</param>
        public LogLevelAttribute(LogLevel level)
        {
            Level = level;
        }
    }
}
