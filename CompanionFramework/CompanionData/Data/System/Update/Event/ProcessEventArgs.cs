using System;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Base class for arguments by all process events
	/// </summary>
	public abstract class ProcessEventArgs : EventArgs
	{
		public readonly RepositoryData repositoryData;

		public ProcessEventArgs(RepositoryData repositoryData)
		{
			this.repositoryData = repositoryData;
		}
	}
}
