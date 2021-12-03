using Companion.Data.System.Update;

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

	private UpdateState currentState;
	private UpdateStateData stateData = new UpdateStateData();

	public void Start()
	{
		currentState = UpdateState.RetrieveGameSystemIndex;
		ExecuteProcess(GetLoadingProcess(currentState));
	}

	/// <summary>
	/// Add repository to the update manager
	/// </summary>
	public void AddRepository()
	{

	}

	public void ExecuteProcess(IUpdateProcess process)
	{
		process.Execute(stateData);
	}

	private IUpdateProcess GetLoadingProcess(UpdateState state)
	{
		if (state == UpdateState.RetrieveGameSystemIndex)
		{
			return new RetrieveGameSystemIndexProcess();
		}
		else
		{
			return null;
		}
	}
}
