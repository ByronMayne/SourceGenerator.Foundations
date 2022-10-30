using System;
using System.Runtime.InteropServices;

namespace SGF.Interop
{
	internal class MessageFilter : IOleMessageFilter
	{
		private const int s_handled = 0;
		private const int s_retryAllowed = 2;
		private const int s_retry = 99;
		private const int s_cancel = -1;
		private const int s_waitAndDispatch = 2;

		/// <param name="IOleMessageFilter">
		int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo) => s_handled;

		/// <param name="IOleMessageFilter">
		int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType) => dwRejectType == s_retryAllowed ? s_retry : s_cancel;

		/// <param name="IOleMessageFilter">
		int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType) => s_waitAndDispatch;

		/// <summary>
		/// Registers this assembly to COM and sets it up for interception of events 
		/// </summary>
		public static void Register()
			=> CoRegisterMessageFilter(new MessageFilter());

		/// <summary>
		/// Removes this assembly an a com message interceptor. 
		/// </summary>
		public static void Revoke()
			=> CoRegisterMessageFilter(null);

		/// <summary>
		/// Set the new com filter 
		/// </summary>
		private static void CoRegisterMessageFilter(IOleMessageFilter? newFilter)
			=> CoRegisterMessageFilter(newFilter, out IOleMessageFilter oldFilter);

		[DllImport("Ole32.dll")]
		private static extern int CoRegisterMessageFilter(IOleMessageFilter? newFilter, out IOleMessageFilter oldFilter);
	}
}
