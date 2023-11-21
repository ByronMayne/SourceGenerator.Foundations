namespace SGF.Diagnostics
{
    /// <summary>
    /// A log sink is a location that events can be written to
    /// </summary>
    public interface ILogSink
    {
        /// <summary>
        /// Writes the log to a given sink
        /// </summary>
        /// <param name="logLevel">The level of the log</param>
        /// <param name="message">The message to write out</param>
        void Write(LogLevel logLevel, string message);
    }
}
