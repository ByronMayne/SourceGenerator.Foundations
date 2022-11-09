namespace SGF.Logging
{
    /// <summary>
    /// Defines the different logging levels that can be produced 
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Verbose is the noisiest level, rarely (if ever) enabled for a production app.
        /// </summary>
        Verbose,

        /// <summary>
        /// Debug is used for internal system events that are not necessarily observable from the outside, but useful when determining how something happened.
        /// </summary>
        Debug,

        /// <summary>
        ///  Information events describe things happening in the system that correspond to its responsibilities and functions. Generally these are the observable actions the system can perform.
        /// </summary>
        Information,

        /// <summary>
        /// When service is degraded, endangered, or may be behaving outside of its expected parameters, Warning level events are used.
        /// </summary>
        Warning,

        /// <summary>
        ///  When functionality is unavailable or expectations broken, an Error event is used.
        /// </summary>
        Error,

        /// <summary>
        /// The most critical level, Fatal events demand immediate attention.
        /// </summary>
        Fatal
    }
}
