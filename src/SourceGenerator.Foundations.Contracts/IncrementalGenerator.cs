#nullable enable
using Microsoft.CodeAnalysis;
using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;

namespace SGF
{
	/// <summary>
	/// Used as a base class for creating your own source generator. This class provides some helper
	/// methods and impoved debugging expereince.
	/// </summary>
	public abstract class IncrementalGenerator : IIncrementalGenerator, IDisposable
	{
		protected static readonly AppDomain s_currentDomain;

		/// <summary>
		/// Gets the name of the source generator
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the log that can allow you to output information to your
		/// IDE of choice
		/// </summary>
		public ILogger Logger { get; }

		static IncrementalGenerator()
		{
			s_currentDomain = AppDomain.CurrentDomain;
		}

		/// <summary>
		/// Initializes a new instance of the incremental generator with an optional name
		/// </summary>
		protected IncrementalGenerator(string? name)
		{
			Name = name ?? GetType().Name;
			Logger = DevelopmentEnviroment.Logger.ForContext(Constants.SourceContextPropertyName, Name);
			Logger.Debug("Initalizing {GeneratorName}", name ?? GetType().Name);
			s_currentDomain.ProcessExit += OnProcessExit;
			s_currentDomain.UnhandledException += OnUnhandledException;

		}

		/// <summary>
		/// Implement to initalize the incremental source generator
		/// </ summary >
		protected abstract void OnInitialize(SgfInitializationContext context);

		/// <summary>
		/// Override to add logic for disposing this instance and free resources
		/// </summary>
		protected virtual void Dipose()
		{
			if(Debugger.IsAttached && Environment.UserInteractive)
			{
				Console.WriteLine("Press any key to quit...");
				Console.ReadKey();
			}
		}

		/// <summary>
		/// Attaches the debugger automtically if you are running from Visual Studio. You have the option
		/// to stop or just continue
		/// </summary>
		protected void AttachDebugger()
		{
			Process process = Process.GetCurrentProcess();
			_ = DevelopmentEnviroment.AttachDebugger(process.Id);
		}

		/// <summary>
		/// Raised when one of the generator functions throws an unhandle exception. Override this to define your own behaviour 
		/// to handle the exception. 
		/// </summary>
		/// <param name="exception">The exception that was thrown</param>
		protected virtual void OnException(Exception exception)
		{
			Logger.Error(exception, "Unhandled exception was throw while running the generator {Name}", Name);
		}

		/// <summary>
		/// Raised when the process is closing, giving us a chance to cleanup any resources
		/// </summary>
		private void OnProcessExit(object sender, EventArgs e)
		{
			try
			{
				Dipose();
				s_currentDomain.ProcessExit -= OnProcessExit;
				s_currentDomain.UnhandledException -= OnUnhandledException;
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Exception thrown while dispoing '{Name}'", Name);
			}
			Log.CloseAndFlush();
		}

		/// <summary>
		/// Events raised when the exception is being thrown by the app domain 
		/// </summary>
		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if(e.ExceptionObject is Exception exception)
			{
				OnException(exception);
			}
		}

		/// <inheritdoc cref="IDisposable"/>
		void IDisposable.Dispose()
			=> Dipose();

		/// <inheritdoc cref = "IIncrementalGenerator" />
		void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
		{
			try
			{
				SgfInitializationContext sgfContext = new SgfInitializationContext(context, OnException);

				OnInitialize(sgfContext);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Error! An unhandle exception was thrown while initializing the source generator '{Name}'.", Name);
			}
		}
	}
}
