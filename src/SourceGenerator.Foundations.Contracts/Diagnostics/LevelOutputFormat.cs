using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SGF.Diagnostics
{
    public static class LogLevelExtensions
    {
        private static IDictionary<LogLevel, string> s_monikersMap;

        static LogLevelExtensions()
        {
            s_monikersMap = new Dictionary<LogLevel, string>()
            {
                [LogLevel.Verbose] = "VRB" ,
                [LogLevel.Debug] = "DBG",
                [LogLevel.Information] = "INF",
                [LogLevel.Warning] = "WRN",
                [LogLevel.Error] = "ERR",
                [LogLevel.Fatal] = "FTL"
            };
        }

        /// <summary>
        /// Gets the monitor or display version of the log level 
        /// </summary>
        public static string GetMoniker(this LogLevel logLevel)
            => s_monikersMap[logLevel];

    }

}
