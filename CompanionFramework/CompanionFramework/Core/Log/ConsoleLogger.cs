using System;

namespace CompanionFramework.Core.Log
{
	public class ConsoleLogger : ILogger
	{
		public ConsoleLogger()
		{
		}

		public void MessageLog(string message)
		{
			Console.WriteLine(message);
		}

		public void MessageLogWarning(string message)
		{
			Console.WriteLine(message);
		}

		public void MessageLogError(string message)
		{
			Console.WriteLine("Error: " + message);
		}

		public void MessageLogException(Exception exception)
		{
			Console.WriteLine("Exception: " + exception.Message);
		}
	}
}
