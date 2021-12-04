using Companion.Data.System.Update;
using CompanionFramework.Core.Threading.Messaging;
using System;

public class UpdateManager
{
	private static UpdateManager instance;
	public static UpdateManager Instance
	{
		get
		{
			if (instance == null)
				instance = new UpdateManager();

			return instance;
		}
	}

	//private UpdateState currentState;
	private UpdateStateData stateData = new UpdateStateData();

	/// <summary>
	/// Fired when game system index is received. Source is UpdateStateData.
	/// </summary>
	public event EventHandler OnGameSystemIndexReceived;

	/// <summary>
	/// Fired when repository index is received. Source is UpdateStateData.
	/// </summary>
	public event EventHandler OnRepositoryIndexReceived;

	public void Start()
	{
		//currentState = UpdateState.RetrieveGameSystemIndex;
		//ExecuteProcess(GetLoadingProcess(currentState));
	}

	public CoreUpdateProcess RetrieveRepositoryIndex(string url, bool async = true)
	{
		RetrieveGameSystemIndexProcess process = new RetrieveGameSystemIndexProcess(url, async);

		process.LoadingComplete += (object result) =>
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnGameSystemIndexReceived, stateData);
			}
			else
			{
				if (OnGameSystemIndexReceived != null)
					OnGameSystemIndexReceived(stateData, null);
			}
		};

		process.Execute(stateData);

		return process;
	}

	/// <summary>
	/// Add repository to the update manager
	/// </summary>
	public void AddRepository()
	{

	}

	//public void ExecuteProcess(IUpdateProcess process)
	//{
	//	process.Execute(stateData);
	//}

	//private IUpdateProcess GetLoadingProcess(UpdateState state)
	//{
	//	if (state == UpdateState.RetrieveGameSystemIndex)
	//	{
	//		return new RetrieveGameSystemIndexProcess();
	//	}
	//	else
	//	{
	//		return null;
	//	}
	//}
}
