using CompanionFramework.Core.Log;
using CompanionFramework.Core.Threading.Messaging;
using System;
using System.Collections.Generic;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Storing the linked data as a data index always needs a repository.
	/// </summary>
	public class GameSystemData
	{
		public readonly Repository repository;
		public readonly DataIndex dataIndex;

		public GameSystemData(Repository repository, DataIndex dataIndex)
		{
			this.repository = repository;
			this.dataIndex = dataIndex;
		}
	}

	/// <summary>
	/// Class to hold repository data as it gets loaded.
	/// </summary>
	public class RepositoryData
	{
		/// <summary>
		/// The url to read the repository index from.
		/// </summary>
		public string url;

		/// <summary>
		/// Repository index if loaded, can be null if not loaded.
		/// </summary>
		public RepositoryIndex repositoryIndex;

		/// <summary>
		/// Data index if loaded, can be null if not loaded. Repository Index must be loaded before this can be loaded.
		/// </summary>
		public readonly Dictionary<string, GameSystemData> dataIndices = new Dictionary<string, GameSystemData>();

		#region Events
		/// <summary>
		/// Fired when data index is received from a repository. Source is <see cref="RepositoryData"/>. EventArgs is <see cref="DataIndexSuccessEventArgs"/>.
		/// </summary>
		public event EventHandler OnDataIndexAdded;

		/// <summary>
		/// Fired when it failed to receive the data index from a repository. Source is <see cref="RepositoryData"/>. EventArgs is <see cref="ProcessFailedEventArgs"/>.
		/// </summary>
		public event EventHandler OnDataIndexFailed;

		/// <summary>
		/// Fired when repository index is received. Source is <see cref="RepositoryData"/>. EventArgs is <see cref="RepositoryIndexSuccessEventArgs"/>.
		/// </summary>
		public event EventHandler OnRepositoryIndexAdded;

		/// <summary>
		/// Fired when it failed to receive the repository index. Source is <see cref="RepositoryData"/>. EventArgs is <see cref="ProcessFailedEventArgs"/>.
		/// </summary>
		public event EventHandler OnRepositoryIndexFailed;
		#endregion

		public RepositoryData()
		{
		}

		public RepositoryData(string url)
		{
			this.url = url;
		}

		/// <summary>
		/// Add Game System with <see cref="DataIndex"/>.
		/// </summary>
		/// <param name="repository">Repository</param>
		/// <param name="dataIndex">Data index</param>
		/// <returns>Game System Data</returns>
		public GameSystemData AddGameSystem(Repository repository, DataIndex dataIndex)
		{
			// fallback to data index name
			if (repository == null)
			{
				// attempt to get it from repository
				if (repositoryIndex != null)
					repository = repositoryIndex.GetRepositoryByName(dataIndex.name);
			}

			if (repository != null)
			{
				GameSystemData gameSystemData = new GameSystemData(repository, dataIndex);
				dataIndices[repository.name] = gameSystemData;
				return gameSystemData;
			}
			else
			{
				FrameworkLogger.Error("Unable to add game system");
				return null;
			}
		}

		/// <summary>
		/// Get the game system data for this repository if it's present. If it's not loaded returns null.
		/// </summary>
		/// <param name="repository">Repository to get the data from</param>
		/// <returns>Game system data</returns>
		public GameSystemData GetGameSystem(Repository repository)
		{
			dataIndices.TryGetValue(repository.name, out GameSystemData gameSystemData);
			return gameSystemData;
		}

		public Repository GetRepositoryByName(string repositoryName)
		{
			return repositoryIndex.GetRepositoryByName(repositoryName);
		}

		public void FireDataIndexAdded(EventArgs eventArgs)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnDataIndexAdded, this, eventArgs);
			}
			else
			{
				if (OnDataIndexAdded != null)
					OnDataIndexAdded(this, eventArgs);
			}
		}

		public void FireDataIndexFailed(EventArgs eventArgs)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnDataIndexFailed, this, eventArgs);
			}
			else
			{
				if (OnDataIndexFailed != null)
					OnDataIndexFailed(this, eventArgs);
			}
		}

		public void FireRepositoryIndexAdded(EventArgs eventArgs)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnRepositoryIndexAdded, this, eventArgs);
			}
			else
			{
				if (OnRepositoryIndexAdded != null)
					OnRepositoryIndexAdded(this, eventArgs);
			}
		}

		public void FireRepositoryIndexFailed(EventArgs eventArgs)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnRepositoryIndexFailed, this, eventArgs);
			}
			else
			{
				if (OnRepositoryIndexFailed != null)
					OnRepositoryIndexFailed(this, eventArgs);
			}
		}
	}
}
