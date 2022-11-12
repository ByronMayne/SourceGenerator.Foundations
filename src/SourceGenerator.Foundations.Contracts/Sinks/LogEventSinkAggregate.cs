using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SGF.Sinks
{
    /// <summary>
    /// A instance of <see cref="ILogEventSink"/> that contains nested sinks
    /// that can be added and removed at runtime
    /// </summary>
    internal class LogEventSinkAggregate : ILogEventSink
    {
        private readonly List<ILogEventSink> m_sinks;

        public LogEventSinkAggregate()
        {
            m_sinks = new List<ILogEventSink>();
            //AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
        }

        /// <summary>
        /// Adds a new sink
        /// </summary>
        public void Add(ILogEventSink sink)
        {
            m_sinks.Add(sink);
        }

        /// <inheritdoc cref="ILogEventSink"/>
        public void Emit(LogEvent logEvent)
        {
            for (int i = 0; i < m_sinks.Count; i++)
            {
                ILogEventSink eventSink = m_sinks[i];
                eventSink.Emit(logEvent);
            }
        }

        ///// <summary>
        ///// Invoked whenever an assembly is loaded and we use this to scan for custom loggers
        ///// </summary>
        //private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        //{
        //    Assembly assembly = args.LoadedAssembly;
        //    AssemblyName assemblyName = assembly.GetName();

        //    if (assembly.FullName.StartsWith("SourceGenerator.Foundations"))
        //    {
        //        Log.Debug("Scanning assembly {AssemblyName} for custom logger", assemblyName.Name);
        //        foreach (Type type in assembly.GetTypes())
        //        {
        //            if (type.IsAbstract) continue;
        //            if (!typeof(ILogEventSink).IsAssignableFrom(type)) continue;

        //            ConstructorInfo? defaultConstructor = type.GetConstructor(Type.EmptyTypes);
        //            if(defaultConstructor != null)
        //            {
        //                Log.Information("Adding event sink {SinkType} from assembly {Assembly}", type.Name, assemblyName.Name);
        //                ILogEventSink logEventSink = (ILogEventSink)defaultConstructor.Invoke(Array.Empty<object>());
        //                Add(logEventSink);
        //            }
        //        }
        //    }
        //}
    }
}
