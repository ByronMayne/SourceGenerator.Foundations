using EnvDTE;
using Serilog.Core;
using Serilog.Events;
using System;
using Constants = EnvDTE.Constants;

namespace SGF.Interop.VisualStudio
{
    public class VisualStudioLogEventSink : ILogEventSink
    {
        private static readonly OutputWindow? s_outputWindow;
        private readonly OutputWindowPane? m_outputPane;

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
        internal VisualStudioLogEventSink()
        {
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

            m_outputPane.OutputString(logEvent.RenderMessage());
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

        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void WriteLine(string message)
        {
            if (m_outputPane != null)
            {
                m_outputPane.OutputString($"{message}{Environment.NewLine}");
            }
        }
    }
}