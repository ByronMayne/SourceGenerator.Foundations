using System;

namespace SGF.Diagnostics.Sinks
{
    /// <summary>
    /// Sink used for outputting events to the console 
    /// </summary>
    public class ConsoleSink : ILogSink
    {
        public LogLevel Level { get; set; }

        public void Write(LogLevel logLevel, string message)
        {
            if (Level >= logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($":Error: {message}");
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[Warn] {message}");
                        break;
                    default:
                        Console.WriteLine(message);
                        break;
                }
            }

            Console.ResetColor();

        }
    }
}
