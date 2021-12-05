using System;

namespace Companion.Data.System.Update
{
	public class RepositoryIndexSuccessEventArgs : ProcessEventArgs
	{
		public RepositoryIndex repositoryIndex; // for convenience

		public RepositoryIndexSuccessEventArgs(RepositoryData repositoryData) : base(repositoryData)
		{
			repositoryIndex = repositoryData.repositoryIndex;
		}
	}
}
