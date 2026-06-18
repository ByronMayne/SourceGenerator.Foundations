namespace SGF.Diagnostics
{
    /// <summary>
    /// Defines the different type of logging events that can be shown
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Disables all logging
        /// </summary>
        None = 0,

        /// <summary>
        /// Very detailed trace information for diagnostics
        /// </summary>
        Trace = 1,

        /// <summary>
        /// Detailed debugging information
        /// </summary>
        Debug = 2,

        /// <summary>
        /// General informational messages
        /// </summary>
        Information = 3,

        /// <summary>
        /// Warning messages for potentially harmful situations
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Error messages for failures
        /// </summary>
        Error = 5,
    }
}
