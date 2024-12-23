﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Contains extensions methods for working with System.Process
    /// </summary>
    public static class ProcessUtility
    {
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle,
            int processInformationClass,
            ref ProcessInformation processInformation,
            int processInformationLength,
            out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static Process? GetParentProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr processHandle = currentProcess.Handle;
            return GetParentProcess(processHandle);
        }

        /// <summary>
        /// Gets the parent of this given process if it exits and you have access
        /// </summary>
        /// <param name="process">The process to get the parent of</param>
        /// <returns>The parent process or null if you don't have access, or it does not exist </returns>
        public static Process? GetParent(this Process? process)
        {
            if (process == null) return null;
            IntPtr processHandle = process.Handle;
            return GetParentProcess(processHandle);
        }

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process? GetParentProcess(int id)
        {
            Process process = Process.GetProcessById(id);
            return GetParentProcess(process.Handle);
        }

        /// <summary>
		/// Gets the command line that was used to start the process
		/// </summary>
		public static string GetCommandLine(this Process process)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                ManagementBaseObject? managementBaseObject = objects.Cast<ManagementBaseObject>().FirstOrDefault();
                if (managementBaseObject == null)
                {
                    throw new Exception($"Unable to find process with id {process.Id}");
                }

                return managementBaseObject["CommandLine"].ToString();
            }
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process? GetParentProcess(IntPtr handle)
        {
            ProcessInformation processInformation = new ProcessInformation();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref processInformation, Marshal.SizeOf(processInformation), out returnLength);
            if (status != 0)
                throw new Win32Exception(status);

            try
            {
                return Process.GetProcessById(processInformation.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
    }
}
