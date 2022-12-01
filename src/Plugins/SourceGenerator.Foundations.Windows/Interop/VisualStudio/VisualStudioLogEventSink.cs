using EnvDTE;
using Serilog;
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
        private static readonly string s_outputWindowName;
        private static readonly string s_messageTemplate;
        private static readonly OutputWindow? s_outputWindow;
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
            s_outputWindowName = "SGF: Generator Output";
            s_messageTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        }

        /// <summary>
        /// Initializes a new instance of an output channel with the
        /// Visual Studio output window. 
        /// </summary>
        internal VisualStudioLogEventSink()
        {
            m_templateFormatter = new MessageTemplateTextFormatter(s_messageTemplate, null);

            if (s_outputWindow != null)
            {
                foreach (OutputWindowPane pane in s_outputWindow.OutputWindowPanes)
                {
                    if (Equals(pane.Name, s_outputWindowName))
                    {
                        m_outputPane = pane;
                        break;
                    }
                }
                m_outputPane ??= s_outputWindow.OutputWindowPanes.Add(s_outputWindowName);
            }
        }

        /// <summary>
        /// Clears the output window of all entries
        /// </summary>
        public void Clear()
        {
            if (m_outputPane != null)
            {
                m_outputPane.Clear();
            }
        }

        public void Emit(LogEvent logEvent)
        {
            if (m_outputPane == null)
            {
                return;
            }
            
            using (StringWriter stringWriter = new StringWriter())
            {
                m_templateFormatter.Format(logEvent, stringWriter);
                Write(stringWriter.ToString());
            }
        }

        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void Write(string message)
        {
            if (m_outputPane != null)
            {
                m_outputPane.OutputString(message);
            }
        }
    }
}