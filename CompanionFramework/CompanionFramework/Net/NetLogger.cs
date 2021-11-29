using CompanionFramework.Core.Log;
using System;
using System.Diagnostics;

namespace CompanionFramework.Net
{
	/// <summary>
	/// Wrapper for logging from the network library. Default log level is everything.
	/// </summary>
	public static class NetLogger
	{
		public static LogLevel LogLevel = LogLevel.All;

		[Conditional("DEBUG")]
		public static void Debug(string message)
		{
			if (LogLevel <= LogLevel.Debug)
			{
				FrameworkLogger.Debug(message);
			}
		}

		public static void Message(string message)
		{
			if (LogLevel <= LogLevel.Message)
			{
				FrameworkLogger.Message(message);
			}
		}

		public static void Warning(string message)
		{
			if (LogLevel <= LogLevel.Warning)
			{
				FrameworkLogger.Warning(message);
			}
		}

		public static void Error(string message)
		{
			if (LogLevel <= LogLevel.Error)
			{
				FrameworkLogger.Error(message);
			}
		}

		public static void Exception(Exception e)
		{
			if (LogLevel <= LogLevel.Exception)
			{
				FrameworkLogger.Exception(e);
			}
		}
	}
}
