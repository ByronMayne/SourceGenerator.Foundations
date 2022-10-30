using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DProcess = System.Diagnostics.Process;
using SGF.Diagnostics;

namespace SGF.Interop
{
    internal class VisualStudioInterop
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);


        static VisualStudioInterop()
        {
            DProcess currentProcess = DProcess.GetCurrentProcess();
            DProcess? parentDebugProcess = currentProcess.GetParent();

            if (parentDebugProcess == null)
            {
                throw new Exception("Unable to get the Visual Studio instance that is running");
            }

            ProcessId = parentDebugProcess.Id;

            switch (parentDebugProcess.ProcessName)
            {
                case "MSBuild":
                    // If it's msbuild we want to stup up
                    parentDebugProcess = parentDebugProcess.GetParent();
                    if (parentDebugProcess != null)
                    {
                        ProcessId = parentDebugProcess.Id;
                    }
                    break;

                case "VsDebugConsole":
                    // In this case Visual Studio reuses the same window without closing it. We have to find the owning instance
                    // The command line parameters contain the visual studio PID that owns it
                    string commandLine = parentDebugProcess.GetCommandLine();
                    Match processIdMatch = Regex.Match(commandLine, @"\\Microsoft-VisualStudio-Debug-Console-(?<ProcessId>\d*)");

                    if (processIdMatch.Success)
                    {
                        ProcessId = int.Parse(processIdMatch.Groups["ProcessId"].Value);
                    }
                    else
                    {
                        ProcessId = -1;
                    }
                    break;
            }
            Version = Environment.GetEnvironmentVariable("VisualStudioVersion");
            HasEnvironment = !string.IsNullOrWhiteSpace(Version);
        }

        /// <summary>
        /// Gets if the current process is running inside a visual studio environment 
        /// </summary>
        public static bool HasEnvironment { get; }

        /// <summary>
        /// Gets the current version of Visual Studio
        /// </summary>
        public static string Version { get; }

        /// <summary>
        /// Gets the process id of Visual Studio
        /// </summary>
        public static int ProcessId { get; }

        /// <summary>
        /// Attaches Visual Studio debugger to the current active process
        /// </summary>
        public static void AttachDebugger()
        {
            DProcess currentProcess = DProcess.GetCurrentProcess();
            int currentProcessId = currentProcess.Id;
            MessageFilter.Register();
			Process? dteProcess = GetProcess(currentProcessId);
            if (dteProcess == null)
            {
			    throw new Exception("Unable to find DTE local process to attach too");
			}
			dteProcess.Attach();
        }

        /// <summary>
		/// Gets the DTE process based on the process id
		/// </summary>
		/// <param name="processId">The id of the process that you want the local DTE process for </param>
		/// <returns>The DTE wrapper</returns>
		private static Process? GetProcess(int processId)
		{
			DTE? dte = VisualStudioInterop.GetDTE();

			if (dte != null)
			{
				IEnumerable<Process> localProcesses = dte.Debugger.LocalProcesses.OfType<Process>();
				return localProcesses.FirstOrDefault(x => x.ProcessID == processId);
			}
			return null;
		}
        
        /// <summary>
        /// Gets the instance of DTE for the current instance of Visual Studio. This code is a bit ugly 
        /// but makes sure that we are attaching this instance of VS and not other one running on the users machine.
        /// </summary>
        public static DTE? GetDTE()
        {
            if (!HasEnvironment)
            {
                return null;
            }

            string programId = $"!VisualStudio.DTE.{Version}:{ProcessId}";
            object? runningObject = null;

            IBindCtx? bindCtx = null;
            IRunningObjectTable? rot = null;
            IEnumMoniker? enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                uint numberFetched = 0;
                while (enumMonikers.Next(1, moniker, out numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string? name = null;

                    try
                    {
                        runningObjectMoniker?.GetDisplayName(bindCtx, null, out name);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    if (!string.IsNullOrEmpty(name) && string.Equals(name, programId, StringComparison.Ordinal))
                    {
                        rot.GetObject(runningObjectMoniker, out runningObject);
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return runningObject as DTE;
        }
    }
}