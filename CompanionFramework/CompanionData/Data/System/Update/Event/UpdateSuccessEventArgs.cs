namespace Companion.Data.System.Update
{
	public class UpdateSuccessEventArgs : ProcessEventArgs
	{
		public readonly GameSystemData gameSystemData;

		public UpdateSuccessEventArgs(RepositoryData repositoryData, GameSystemData gameSystemData) : base(repositoryData)
		{
			this.gameSystemData = gameSystemData;
		}
	}
}
