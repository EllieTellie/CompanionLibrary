using System;

namespace Companion.Data.System.Update
{
	public class DataIndexSuccessEventArgs : ProcessEventArgs
	{
		public readonly Repository repository;
		public readonly DataIndex dataIndex;

		public DataIndexSuccessEventArgs(RepositoryData repositoryData, Repository repository, DataIndex dataIndex) : base(repositoryData)
		{
			this.repository = repository;
			this.dataIndex = dataIndex;
		}
	}
}
