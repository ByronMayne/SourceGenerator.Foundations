using EnvDTE;
using SGF.Diagnostics;
using System;
using System.Runtime.InteropServices;
using Constants = EnvDTE.Constants;

namespace SGF.Interop.VisualStudio
{
    public class VisualStudioLogEventSink : ILogSink
    {
        private const string outputPanelName = "Source Generators";

        /// https://learn.microsoft.com/en-us/visualstudio/extensibility/ide-guids?view=vs-2022
        private const string BUILD_OUTPUT_PANE_GUID = "{1BD8A850-02D1-11D1-BEE7-00A0C913D1F8}";

        private readonly DTE? m_dte;
        private readonly OutputWindow? m_outputWindow;
        private readonly OutputWindowPane? m_buildOutput;
        private readonly OutputWindowPane? m_sourceGeneratorOutput;
        
        private static bool s_clearedOuput;

        static VisualStudioLogEventSink()
        {
            s_clearedOuput = false;
        }

        /// <summary>
        /// Initializes a new instance of an output channel with the
        /// Visual Studio output window. 
        /// </summary>
        internal VisualStudioLogEventSink()
        {
            m_buildOutput = null;

            try
            {
                // TODO: Try to load DTE at a later point
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
    
                    m_sourceGeneratorOutput ??= m_outputWindow.OutputWindowPanes.Add(outputPanelName);

                    // When adding a pane will steal focus, we don't want this. So lets force it back to build
                    foreach (OutputWindowPane pane in m_outputWindow.OutputWindowPanes)
                    {
                        if(string.Equals(BUILD_OUTPUT_PANE_GUID, pane.Guid))
                        {
                            pane.Activate();
                        }
                    }
                }

                if(!s_clearedOuput && m_sourceGeneratorOutput != null)
                {
                    s_clearedOuput = true;
                    m_sourceGeneratorOutput?.Clear();
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
            message += "\n";

            if (logLevel >= LogLevel.Warning)
            {
               Write(message, m_buildOutput);
            }

            Write(message, m_sourceGeneratorOutput);
        }


        /// <summary>
        /// Writes an entry to the output window if it has been initialized
        /// </summary>
        public void Write(string message, OutputWindowPane? outputPain)
        {
            try
            {
                outputPain?.OutputString(message);
            }
            catch (COMException)
            {
                // When the person is debugging Visual Studio will be busy and this will throw this exception 
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Exception was thrown while writing log. Please report this on github {exception}");
            }
        }
    }
}