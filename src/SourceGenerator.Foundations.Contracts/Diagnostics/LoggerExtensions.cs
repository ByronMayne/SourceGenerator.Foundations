using System;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Contains all the extension methods for working with a log
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs verbose information to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="exception">The exception that was produced</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogVerbose(this ILogger logger, Exception exception, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Verbose, exception, messageTemplate, arguments);

        /// <summary>
        /// Logs verbose information to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogVerbose(this ILogger logger, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Verbose, null, messageTemplate, arguments);

        /// <summary>
        /// Logs debug information to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="exception">The exception that was produced</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogDebug(this ILogger logger, Exception exception, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Debug, exception, messageTemplate, arguments);

        /// <summary>
        /// Logs debug information to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogDebug(this ILogger logger, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Debug, null, messageTemplate, arguments);

        /// <summary>
        /// Logs information to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="exception">The exception that was produced</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogInformation(this ILogger logger, Exception exception, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Information, exception, messageTemplate, arguments);

        /// <summary>
        /// Logs information to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogInformation(this ILogger logger, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Information, null, messageTemplate, arguments);


        /// <summary>
        /// Logs a warning to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="exception">The exception that was produced</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogWarning(this ILogger logger, Exception exception, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Warning, exception, messageTemplate, arguments);

        /// <summary>
        /// Logs a warning to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogWarning(this ILogger logger, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Warning, null, messageTemplate, arguments);

        /// <summary>
        /// Logs a error to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="exception">The exception that was produced</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogError(this ILogger logger, Exception exception, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Error, exception, messageTemplate, arguments);

        /// <summary>
        /// Logs a error to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogError(this ILogger logger, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Error, null, messageTemplate, arguments);

        /// <summary>
        /// Logs a fatal message to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="exception">The exception that was produced</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogFatal(this ILogger logger, Exception exception, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Fatal, exception, messageTemplate, arguments);

        /// <summary>
        /// Logs a fatal message to the log 
        /// </summary>
        /// <param name="logger">The logger instance to forward the message to.</param>
        /// <param name="messageTemplate">The template of the message to write out where arguments will be subsituted in.</param>
        /// <param name="arguments">The arguments that will be used to fill the wholes in the template</param>
        public static void LogFatal(this ILogger logger, string messageTemplate, params object[] arguments)
            => logger.Log(LogLevel.Fatal, null, messageTemplate, arguments);
    }
}
