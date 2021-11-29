using System;

namespace CompanionFramework.Core.Log
{
	/// <summary>
	/// Interface for logging framework.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Log a message.
		/// </summary>
		/// <param name="message">message</param>
		void MessageLog(string message);

		/// <summary>
		/// Log a warning.
		/// </summary>
		/// <param name="message">message</param>
		void MessageLogWarning(string message);

		/// <summary>
		/// Log an error.
		/// </summary>
		/// <param name="message">message</param>
		void MessageLogError(string message);

		/// <summary>
		/// Log an exception.
		/// </summary>
		/// <param name="exception">message</param>
		void MessageLogException(Exception exception);
	}
}