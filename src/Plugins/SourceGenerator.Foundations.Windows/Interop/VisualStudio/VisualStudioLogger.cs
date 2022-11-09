using EnvDTE;
using SGF.Interop.VisualStudio;
using SGF.Logging;
using System;

namespace SGF
{
    public class VisualStudioLogger : ILogger
    {
        private static readonly OutputWindow? s_outputWindow;
        private OutputWindowPane? m_outputPane;

        static VisualStudioLogger()
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
        internal VisualStudioLogger(string context)
        {
            if (s_outputWindow != null)
            {
                foreach (OutputWindowPane pane in s_outputWindow.OutputWindowPanes)
                {
                    if (Equals(pane.Name, context))
                    {
                        m_outputPane = pane;
                        break;
                    }
                }
                m_outputPane ??= s_outputWindow.OutputWindowPanes.Add(context);
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

        /// <inheritdoc cref="ILogger"/>
        public void Log(LogLevel logLevel, Exception? exception, string messageTemplate, object[] arguments)
        {
            if(m_outputPane == null)
            {
                return;
            }

            string renderedMessage = string.Format(messageTemplate, arguments);
            m_outputPane.OutputString($"{renderedMessage}{Environment.NewLine}");
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
                m_outputPane.OutputString($"{message}{System.Environment.NewLine}");
            }
        }
    }
}