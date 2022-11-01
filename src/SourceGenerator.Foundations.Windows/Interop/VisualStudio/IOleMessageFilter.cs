using System;
using System.Runtime.InteropServices;

namespace SGF.Interop.VisualStudio
{
    /// <summary>
    /// A low-level interface for allowing a class process to process and filter messages from other COM assemblies. 
    /// </summary>
    [ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleMessageFilter
    {
        /// <summary>
        /// Provides a single entry point for incoming calls
        /// </summary>
        /// <param name="dwCallType">The type of incoming call that has been received. Possible values are from the enumeration CALLTYPE.</param>
        /// <param name="hTaskCaller">The thread id of the caller.</param>
        /// <param name="dwTickCount">The elapsed tick count since the outgoing call was made, if dwCallType is not CALLTYPE_TOPLEVEL. If dwCallType is CALLTYPE_TOPLEVEL, dwTickCount should be ignored.</param>
        /// <param name="lpInterfaceInfo">A pointer to an INTERFACEINFO structure that identifies the object, interface, and method being called. In the case of DDE calls, lpInterfaceInfo can be NULL because the DDE layer does not return interface information.</param>
        [PreserveSig]
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

        /// <summary>
        /// Provides the opportunity to react to a rejected COM call.
        /// </summary>
        /// <param name="hTaskCallee">The thread id of the called application.</param>
        /// <param name="dwTickCount">The number of elapsed ticks since the call was made.</param>
        /// <param name="dwRejectType">Specifies either SERVERCALL_REJECTED or SERVERCALL_RETRYLATER, as returned by the object application.</param>
        [PreserveSig]
        int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);

        /// <summary>
        /// Indicates that a message has arrived while COM is waiting to respond to a remote call.
        /// </summary>
        /// <param name="hTaskCallee">The thread id of the called application.</param>
        /// <param name="dwTickCount"The number of ticks since the call was made. It is calculated from the GetTickCount function.></param>
        /// <param name="dwPendingType">The type of call made during which a message or event was received. Possible values are from the enumeration PENDINGTYPE, where PENDINGTYPE_TOPLEVEL means the outgoing call was not nested within a call from another application and PENDINTGYPE_NESTED means the outgoing call was nested within a call from another application.</param>
        [PreserveSig]
        int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
    }
}
