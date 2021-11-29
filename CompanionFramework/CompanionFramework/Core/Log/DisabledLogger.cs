using System;

namespace CompanionFramework.Core.Log
{
	/// <summary>
	/// The disabled logger does exactly what is says on the tin.
	/// </summary>
	public class DisabledLogger : ILogger
	{
		public void MessageLog(string message)
		{
		}

		public void MessageLogError(string message)
		{
		}

		public void MessageLogException(Exception exception)
		{
		}

		public void MessageLogWarning(string message)
		{
		}
	}
}
