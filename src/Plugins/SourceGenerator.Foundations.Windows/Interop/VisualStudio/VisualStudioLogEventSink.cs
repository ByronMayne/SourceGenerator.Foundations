using EnvDTE;
using SGF.Diagnostics;
using System;
using System.IO;
using Constants = EnvDTE.Constants;

namespace SGF.Interop.VisualStudio
{
    public class VisualStudioLogEventSink : ILogSink
    {
        private const string outputPanelName = "Source Generators";

        /// https://learn.microsoft.com/en-us/visualstudio/extensibility/ide-guids?view=vs-2022
        private const string BUILD_OUTPUT_PANE_GUID = "1BD8A850-02D1-11D1-BEE7-00A0C913D1F8";

        private readonly DTE? m_dte;
        private readonly OutputWindow? m_outputWindow;
        private readonly OutputWindowPane? m_buildOutput;
        private readonly OutputWindowPane? m_sourceGeneratorOutput;

        /// <summary>
        /// Initializes a new instance of an output channel with the
        /// Visual Studio output window. 
        /// </summary>
        internal VisualStudioLogEventSink()
        {
            m_buildOutput = null;

            try
            {
                // TODO: Try to load DTE a at a later point
                m_dte = VisualStudioInterop.GetDTE();
                if (m_dte != null)
                {
                    Window window = m_dte.Windows.Item(Constants.vsWindowKindOutput);
                    m_outputWindow = window.Object as OutputWindow;
                }

                if (m_outputWindow != null)
                {
                    foreach (OutputWindowPane pane in m_outputWindow.OutputWindowPanes)
                    {
                        string name = pane.Name;
                        if (Equals(name, outputPanelName) && pane.DTE == m_dte)
                        {
                            m_sourceGeneratorOutput = pane;
                        }

                        if (string.Equals(BUILD_OUTPUT_PANE_GUID, pane.Guid))
                        {
                            m_buildOutput = pane;
                        }
                    }
                    // Store the currently active window, because adding a new one always draws focus, we don't want that 
                    OutputWindowPane currentActive = m_outputWindow.ActivePane;
                    m_sourceGeneratorOutput ??= m_outputWindow.OutputWindowPanes.Add(outputPanelName);
                    currentActive.Activate(); // Set previous one to active
                }
            }
            catch
            {
                // nothing
            }
        }

        /// <summary>
        /// Clears the output window of all entries
        /// </summary>
        public void Clear()
        {
            m_sourceGeneratorOutput?.Clear();
        }

        public void Write(LogLevel logLevel, string message)
        {
            if (logLevel >= LogLevel.Warning)
            {
                m_buildOutput?.OutputString(message);
            }

            m_sourceGeneratorOutput?.OutputString(message);
        }


        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void Write(string message)
        {
            m_sourceGeneratorOutput?.OutputString(message);
        }

     

        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void WriteLine(string message)
        {
            m_sourceGeneratorOutput?.OutputString($"{message}{Environment.NewLine}");
        }
    }
}