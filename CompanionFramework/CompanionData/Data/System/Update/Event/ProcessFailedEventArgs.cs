using System;

namespace Companion.Data.System.Update
{
	public class ProcessFailedEventArgs : ProcessEventArgs
	{
		public readonly UpdateError error;
		public readonly string message;

		public ProcessFailedEventArgs(RepositoryData repositoryData, UpdateError error, string message) : base(repositoryData)
		{
			this.error = error;
			this.message = message;
		}
	}
}
