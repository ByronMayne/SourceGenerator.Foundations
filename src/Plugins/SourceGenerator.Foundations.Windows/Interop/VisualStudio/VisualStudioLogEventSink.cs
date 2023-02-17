using EnvDTE;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.IO;
using Constants = EnvDTE.Constants;

namespace SGF.Interop.VisualStudio
{
    public class VisualStudioLogEventSink : ILogEventSink
    {
        const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        private static readonly OutputWindow? s_outputWindow;
        private readonly bool m_outputInitialized;
        private readonly OutputWindowPane? m_outputPane;
        private readonly MessageTemplateTextFormatter m_templateFormatter;

        static VisualStudioLogEventSink()
        {
            DTE? dte = VisualStudioInterop.GetDTE();
            if (dte != null)
            {
                Window window = dte.Windows.Item(Constants.vsWindowKindOutput);
                s_outputWindow = window.Object as OutputWindow;
            }
        }

        /// <summary>
        /// Initializes a new instance of an output channel with the
        /// Visual Studio output window. 
        /// </summary>
        internal VisualStudioLogEventSink(
            IFormatProvider? formatProvider = null,
            string outputTemplate = DefaultOutputTemplate)
        {
            m_outputInitialized = false;
            m_templateFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            if (s_outputWindow != null)
            {
                foreach (OutputWindowPane pane in s_outputWindow.OutputWindowPanes)
                {
                    if (Equals(pane.Name, "Source Generator"))
                    {
                        m_outputPane = pane;
                        break;
                    }
                }
                m_outputPane ??= s_outputWindow.OutputWindowPanes.Add("Source Generator");
                m_outputInitialized = m_outputPane != null;
            }
        }

        /// <summary>
        /// Clears the output window of all entries
        /// </summary>
        public void Clear()
        {
            if (m_outputInitialized)
            {
                m_outputPane!.Clear();
            }
        }

        public void Emit(LogEvent logEvent)
        {
            if (m_outputInitialized)
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    m_templateFormatter.Format(logEvent, stringWriter);
                    m_outputPane!.OutputString(stringWriter.ToString());
                }
            }
        }

        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void Write(string message)
        {
            if (m_outputInitialized)
            {
                m_outputPane!.OutputString(message);
            }
        }

        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void WriteLine(string message)
        {
            if (m_outputInitialized)
            {
                m_outputPane!.OutputString($"{message}{Environment.NewLine}");
            }
        }
    }
}