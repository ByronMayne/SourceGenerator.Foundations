using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Diagnostics
{
    public static class LoggerExtensions
    {
        public static void AddSink<T>(this ILogger logger) where T: ILogSink, new()
        {
            logger.AddSink(new T());
        }

        public static void Information(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Information, null, message);
        }

        public static void Information(this ILogger logger, Exception exception, string message) 
        {
            logger.Log(LogLevel.Information, exception, message);
        }

        public static void Warning(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, null, message);
        }

        public static void Warning(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Warning, exception, message);
        }

        public static void Error(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Error, null, message);
        }

        public static void Error(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Error, exception, message);
        }
    }
}
