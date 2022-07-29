using Companion.Data;
using Companion.Data.System.Update;
using CompanionFramework.Core.Threading.Messaging;
using System;
using System.Collections.Generic;

/// <summary>
/// Holds data retrieved from the repositories for updates
/// </summary>
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

	#region Events
	/// <summary>
	/// Fired when data index is received from a repository. Source is <see cref="UpdateManager"/>. EventArgs is <see cref="DataIndexSuccessEventArgs"/>.
	/// </summary>
	public event EventHandler OnDataIndexAdded;

	/// <summary>
	/// Fired when it failed to receive the data index from a repository. Source is <see cref="UpdateManager"/>. EventArgs is <see cref="ProcessFailedEventArgs"/>.
	/// </summary>
	public event EventHandler OnDataIndexFailed;

	/// <summary>
	/// Fired when repository index is received. Source is <see cref="UpdateManager"/>. EventArgs is <see cref="RepositoryIndexSuccessEventArgs"/>.
	/// </summary>
	public event EventHandler OnRepositoryIndexAdded;

	/// <summary>
	/// Fired when it failed to receive the repository index. Source is <see cref="UpdateManager"/>. EventArgs is <see cref="ProcessFailedEventArgs"/>.
	/// </summary>
	public event EventHandler OnRepositoryIndexFailed;

	/// <summary>
	/// Fired when repository update was successful. Source is <see cref="UpdateManager"/>. EventArgs is <see cref="UpdateSuccessEventArgs"/>.
	/// </summary>
	public event EventHandler OnUpdateSucceeded;

	/// <summary>
	/// Fired when it failed to update the repository. Source is <see cref="UpdateManager"/>. EventArgs is <see cref="ProcessFailedEventArgs"/>.
	/// </summary>
	public event EventHandler OnUpdateFailed;
	#endregion

	/// <summary>
	/// List of loaded repositories.
	/// </summary>
	protected List<RepositoryData> loadedRepositories = new List<RepositoryData>();

    /// <summary>
    /// Load ALL repository data indices into the repository data. This is a brute force method to get every data index, ideally we just cache the repository data to avoid this lookup.
    /// </summary>
    /// <param name="repositoryData">Repository data to load</param>
    /// <param name="async">Whether request should be done asynchronous</param>
    /// <returns>Returns true if it was executed</returns>
	internal bool RetrieveRepositoryDataIndex(RepositoryData repositoryData, bool async = true)
	{
		RepositoryIndex repositoryIndex = repositoryData.repositoryIndex;

		// just get all of them, 140+ requests
		foreach (Repository repository in repositoryIndex.repositories)
        {
            if (!RetrieveRepositoryDataIndex(repositoryIndex, repository, async))
            {
				return false;
			}
        }

		return true;
    }

	/// <summary>
	/// Retrieve the repository index from the source.
	/// </summary>
	/// <param name="url">Source url</param>
	/// <param name="async">Whether request should be done asynchronous</param>
	/// <returns>Returns true if it was executed</returns>
	public bool RetrieveRepositoryIndex(string url, bool async = true)
	{
		RepositoryData repositoryData = AddRepositoryData(url);

		return RetrieveRepositoryIndexTask(repositoryData, async);
	}

    /// <summary>
    /// Retrieve the repository index from the source.
    /// </summary>
    /// <param name="repositoryData">Repository data which will replace any currently cached data</param>
    /// <param name="async">Whether request should be done asynchronous</param>
    /// <returns>Returns true if it was executed</returns>
    public bool RetrieveRepositoryIndex(RepositoryData repositoryData, bool async = true)
	{
		AddRepositoryData(repositoryData);

		return RetrieveRepositoryIndexTask(repositoryData, async);
	}

	protected bool RetrieveRepositoryIndexTask(RepositoryData repositoryData, bool async)
	{
		RetrieveRepositoryIndexProcess process = new RetrieveRepositoryIndexProcess(repositoryData.url, async);

		// handle abort event
		process.LoadingAborted += (UpdateError error, string message) =>
		{
			ProcessFailedEventArgs eventArgs = new ProcessFailedEventArgs(repositoryData, error, message);
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnRepositoryIndexFailed, this, eventArgs);
			}
			else
			{
				if (OnRepositoryIndexFailed != null)
					OnRepositoryIndexFailed(this, eventArgs);
			}
		};

		// handle complete event
		process.LoadingComplete += (object result) =>
		{
			RepositoryIndex repositoryIndex = (RepositoryIndex)result;

			RepositoryIndexSuccessEventArgs eventArgs = new RepositoryIndexSuccessEventArgs(repositoryData);

			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnRepositoryIndexAdded, this, eventArgs);
			}
			else
			{
				if (OnRepositoryIndexAdded != null)
					OnRepositoryIndexAdded(this, eventArgs);
			}
		};

		// start process
		process.Execute(repositoryData);

		return true;
	}

	/// <summary>
	/// Retrieve the data index for the specified repository.
	/// </summary>
	/// <param name="repositoryData">Repository data</param>
	/// <param name="repositoryName">Repository name</param>
	/// <param name="async">Whether request should be done asynchronous</param>
	/// <returns>Returns true if it was executed</returns>
	public bool RetrieveRepositoryDataIndex(RepositoryData repositoryData, string repositoryName, bool async = true)
	{
		Repository repository = repositoryData.GetRepositoryByName(repositoryName);
		if (repository == null)
			return false;

		return RetrieveRepositoryDataIndex(repositoryData, repository, async);
	}

	/// <summary>
	/// Retrieve the data index for the specified repository.
	/// </summary>
	/// <param name="repositoryIndex">Repository index</param>
	/// <param name="repository">Repository</param>
	/// <param name="async">Whether request should be done asynchronous</param>
	/// <returns>Returns true if it was executed</returns>
	public bool RetrieveRepositoryDataIndex(RepositoryIndex repositoryIndex, Repository repository, bool async = true)
	{
		RepositoryData repositoryData = AddRepositoryData(repositoryIndex.repositorySourceUrl);
		return RetrieveRepositoryDataIndex(repositoryData, repository, async);
	}

	/// <summary>
	/// Retrieve the data index for the specified repository.
	/// </summary>
	/// <param name="repositoryData">Repository data</param>
	/// <param name="repository">Repository</param>
	/// <param name="async">Whether request should be done asynchronous</param>
	/// <returns>Returns true if it was executed</returns>
	protected bool RetrieveRepositoryDataIndex(RepositoryData repositoryData, Repository repository, bool async)
	{
		if (repositoryData == null)
			return false;

		RetrieveDataIndexProcess process = new RetrieveDataIndexProcess(repository, async);

		// handle abort event
		process.LoadingAborted += (UpdateError error, string message) =>
		{
			ProcessFailedEventArgs eventArgs = new ProcessFailedEventArgs(repositoryData, error, message);
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnDataIndexFailed, this, eventArgs);
			}
			else
			{
				if (OnDataIndexFailed != null)
					OnDataIndexFailed(this, eventArgs);
			}
		};

		// handle complete event
		process.LoadingComplete += (object result) =>
		{
			GameSystemData gameSystemData = (GameSystemData)result;

			DataIndexSuccessEventArgs eventArgs = new DataIndexSuccessEventArgs(repositoryData, gameSystemData.repository, gameSystemData.dataIndex);

			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnDataIndexAdded, this, eventArgs);
			}
			else
			{
				if (OnDataIndexAdded != null)
					OnDataIndexAdded(this, eventArgs);
			}
		};

		process.Execute(repositoryData);

		return true;
	}

	/// <summary>
	/// Update from a repository.
	/// </summary>
	/// <param name="repositoryData">Repository state data</param>
	/// <param name="repository">Repository to update</param>
	/// <param name="dataPath">Where to save the repository on disk</param>
	/// <param name="async">If true this completes async</param>
	/// <returns>RepositoryUpdate object or null if unable to update</returns>
	public RepositoryUpdate UpdateFromRepository(RepositoryData repositoryData, Repository repository, string dataPath, bool async = true)
	{
		GameSystemData gameSystemData = repositoryData.GetGameSystem(repository);
		return UpdateFromRepository(repositoryData, gameSystemData, repository, dataPath, async);
	}

	protected RepositoryUpdate UpdateFromRepository(RepositoryData repositoryData, GameSystemData gameSystemData, Repository repository, string dataPath, bool async)
	{
		if (repositoryData == null || gameSystemData == null || dataPath == null)
		{
			return null;
		}

		// TODO: we want to maybe retrieve GameSystemData from the server if it's not present here

		UpdateGameSystemProcess process = new UpdateGameSystemProcess(gameSystemData.repository, gameSystemData.dataIndex, dataPath, async);

		RepositoryUpdate repositoryUpdate = new RepositoryUpdate();
		process.SetRepositoryUpdate(repositoryUpdate);

		// handle abort event
		process.LoadingAborted += (UpdateError error, string message) =>
		{
			ProcessFailedEventArgs eventArgs = new ProcessFailedEventArgs(repositoryData, error, message);
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnUpdateFailed, this, eventArgs);
			}
			else
			{
				if (OnUpdateFailed != null)
					OnUpdateFailed(this, eventArgs);
			}
		};

		// handle complete event
		process.LoadingComplete += (object result) =>
		{
			UpdateSuccessEventArgs eventArgs = new UpdateSuccessEventArgs(repositoryData, gameSystemData);

			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnUpdateSucceeded, this, eventArgs);
			}
			else
			{
				if (OnUpdateSucceeded != null)
					OnUpdateSucceeded(this, eventArgs);
			}
		};

		// execute update
		process.Execute(repositoryData);

		return repositoryUpdate;
	}

	/// <summary>
	/// Get the existing repository data by the url if it exists.
	/// </summary>
	/// <param name="url">Repository url</param>
	/// <returns>Repository data or null if not found</returns>
	public RepositoryData GetRepositoryDataByUrl(string url)
	{
		foreach (RepositoryData repositoryData in loadedRepositories)
		{
			if (repositoryData.url == url)
				return repositoryData;
		}

		return null;
	}

	protected void RemoveRepositoryData(string repositoryUrl)
	{
		for (int i=loadedRepositories.Count-1; i>=0; i--)
		{
			RepositoryData repositoryData = loadedRepositories[i];
			if (repositoryData.url == repositoryUrl)
				loadedRepositories.RemoveAt(i); // remove any matching ones
		}
	}

	/// <summary>
	/// Replaces any existing repository data with the new data or just adds it if no existing data is loaded.
	/// </summary>
	/// <param name="repositoryData">Repository data</param>
	protected void AddRepositoryData(RepositoryData repositoryData)
	{
		// remove existing ones
		RemoveRepositoryData(repositoryData.url);

		// add new one
		loadedRepositories.Add(repositoryData);
	}

	/// <summary>
	/// Adds the repository url to the update manager or returns the existing repository data.
	/// </summary>
	/// <param name="repositoryUrl">Repository url</param>
	/// <returns>Repository data</returns>
	public RepositoryData AddRepositoryData(string repositoryUrl)
	{
		RepositoryData repositoryData = GetRepositoryDataByUrl(repositoryUrl);
		if (repositoryData == null)
		{
			repositoryData = new RepositoryData(repositoryUrl);
			loadedRepositories.Add(repositoryData);
		}

		return repositoryData;
	}
}
