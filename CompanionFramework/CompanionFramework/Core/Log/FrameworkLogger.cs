using System;
using System.Diagnostics;

namespace CompanionFramework.Core.Log
{
	/// <summary>
	/// Logs messages through a singleton that is lazily initialized.
	/// By default it uses the <see cref="DisabledLogger"/> and won't log anything. The log level by default is LogLevel.All.
	/// </summary>
	public class FrameworkLogger
	{
		/// <summary>
		/// Singleton instance that starts of null.
		/// </summary>
		private static FrameworkLogger instance;

		private ILogger logger;

		public LogLevel LogLevel
		{
			get; set;
		}

		public FrameworkLogger()
		{
			logger = new DisabledLogger();

			LogLevel = LogLevel.All;
		}

		/// <summary>
		/// Gets the framework logger singleton and creates an instance if required.
		/// </summary>
		/// <returns>Framework logger instance</returns>
		public static FrameworkLogger Instance()
		{
			if (instance == null)
				instance = new FrameworkLogger();

			return instance;
		}

		/// <summary>
		/// Get the logger for the framework. By default logging is disabled.
		/// </summary>
		/// <returns>current logger instance</returns>
		public ILogger GetLogger()
		{
			return logger;
		}

		/// <summary>
		/// Set the logger for the framework.
		/// </summary>
		/// <param name="logger">new logger instance</param>
		public void SetLogger(ILogger logger)
		{
			this.logger = logger;
		}

		[Conditional("DEBUG")]
		public static void Debug(string message)
		{
			FrameworkLogger logger = Instance();
			if (logger.LogLevel <= LogLevel.Debug)
			{
				logger.logger.MessageLog(message);
			}
		}

		public static void Message(string message)
		{
			FrameworkLogger logger = Instance();
			if (logger.LogLevel <= LogLevel.Message)
			{
				logger.logger.MessageLog(message);
			}
		}

		public static void Warning(string message)
		{
			FrameworkLogger logger = Instance();
			if (logger.LogLevel <= LogLevel.Warning)
			{
				logger.logger.MessageLogWarning(message);
			}
		}

		public static void Error(string message)
		{
			FrameworkLogger logger = Instance();
			if (logger.LogLevel <= LogLevel.Error)
			{
				logger.logger.MessageLogError(message);
			}
		}

		public static void Exception(Exception e)
		{
			FrameworkLogger logger = Instance();
			if (logger.LogLevel <= LogLevel.Exception)
			{
				logger.logger.MessageLogException(e);
			}
		}
	}
}
