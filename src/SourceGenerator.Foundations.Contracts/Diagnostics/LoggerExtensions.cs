using SGF.Diagnostics;
using System;

/// <summary>
/// Contains extension methods for working with logging in Source Generator Foundations
/// </summary>
public static class LoggerExtensions
{
    public static void AddSink<T>(this ILogger logger) where T : ILogSink, new()
    {
        logger.AddSink(new T());
    }

    /// <summary>
    /// Adds a new info log entry 
    /// </summary>
    public static void Information(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Information, null, message);
    }

    /// <summary>
    /// Adds a new info log entry with an exception 
    /// </summary>
    public static void Information(this ILogger logger, Exception exception, string message)
    {
        logger.Log(LogLevel.Information, exception, message);
    }

    /// <summary>
    /// Adds a new warning log entry 
    /// </summary>
    public static void Warning(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Warning, null, message);
    }

    /// <summary>
    /// Adds a new warning entry with an exception 
    /// </summary>
    public static void Warning(this ILogger logger, Exception exception, string message)
    {
        logger.Log(LogLevel.Warning, exception, message);
    }

    /// <summary>
    /// Adds a new warning entry
    /// </summary>
    public static void Error(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Error, null, message);
    }

    /// <summary>
    /// Adds a new error entry with an exception 
    /// </summary>
    public static void Error(this ILogger logger, Exception exception, string message)
    {
        logger.Log(LogLevel.Error, exception, message);
    }
}