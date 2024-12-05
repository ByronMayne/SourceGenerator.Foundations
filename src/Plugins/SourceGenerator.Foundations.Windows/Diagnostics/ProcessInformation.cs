using System;
using System.Runtime.InteropServices;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Contains information about a process from Invoking into native
    /// </summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntqueryinformationprocess
    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessInformation
    {
        /// <summary>
        /// Contains the same value that GetExitCodeProcess returns, however the use of GetExitCodeProcess is preferable for clarity and safety.
        /// </summary>
        public IntPtr ExitStatus;

        /// <summary>
        /// Points to a PEB structure.
        /// </summary>
        public IntPtr PebBaseAddress;

        /// <summary>
        /// Can be cast to a DWORD and contains the same value that GetProcessAffinityMask returns for the lpProcessAffinityMask parameter.
        /// </summary>
        public IntPtr AffinityMask;

        /// <summary>
        /// Contains the process priority as described in Scheduling Priorities.
        /// </summary>
        public IntPtr BasePriority;

        /// <summary>
        /// Can be cast to a DWORD and contains a unique identifier for this process. We recommend using the GetProcessId function to retrieve this information.
        /// </summary>
        public IntPtr UniqueProcessId;
        /// <summary>
        /// Can be cast to a DWORD and contains a unique identifier for the parent process.
        /// </summary>
        public IntPtr InheritedFromUniqueProcessId;
    }
}
