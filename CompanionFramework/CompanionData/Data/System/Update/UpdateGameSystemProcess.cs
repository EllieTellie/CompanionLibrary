using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Net.Http;
using CompanionFramework.Net.Http.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Updates a game system from a repository. It will check which files need updating in the data index and download those files only.
	/// </summary>
	public class UpdateGameSystemProcess : CoreUpdateProcess
	{
		protected readonly Repository repository;
		protected readonly DataIndex dataIndex;
		protected readonly string dataPath;
		protected readonly bool async;

		protected RepositoryUpdate repositoryUpdate;

		/// <summary>
		/// Create a new update system process.
		/// </summary>
		/// <param name="repository">Repository to update from</param>
		/// <param name="dataIndex">Data index retrieved from repository</param>
		/// <param name="dataPath">File path where the data should reside</param>
		/// <param name="async">Whether to update async</param>
		public UpdateGameSystemProcess(Repository repository, DataIndex dataIndex, string dataPath, bool async = true)
		{
			this.repository = repository;
			this.dataIndex = dataIndex;
			this.dataPath = dataPath;
			this.async = async;
		}

		/// <inheritdoc/>
		public override void Execute(RepositoryData state)
		{
			if (state == null)
			{
				Abort(UpdateError.MissingState, "Missing repository data");
				return;
			}
			else if (repository == null)
			{
				Abort(UpdateError.InvalidParameter, "Missing repository index");
				return;
			}
			else if (dataIndex == null)
			{
				Abort(UpdateError.InvalidParameter, "Missing data index");
				return;
			}
			else if (dataPath == null)
			{
				Abort(UpdateError.InvalidParameter, "Missing path");
				return;
			}

			// create directory if it does not exist

			try
			{
				if (!Directory.Exists(dataPath))
					Directory.CreateDirectory(dataPath);
			}
			catch (Exception e)
			{
				Abort(UpdateError.FailedFileAccess, e.Message);
				return;
			}

			List<DataIndexEntry> updates = GetUpdateDataIndices();

			// if up to date just complete
			if (updates.Count == 0)
			{
				Complete();
				return;
			}

			List<HttpDownload> downloads = new List<HttpDownload>();
			foreach (DataIndexEntry update in updates)
			{
				HttpDownload download = CreateDownload(update);
				if (download != null) // should never be null most likely
				{
					download.SetAsync(async); // false for debugging
					downloads.Add(download);
				}
			}

			HttpDownloadSet downloadSet = new HttpDownloadSet(true, downloads.ToArray());
			downloadSet.DownloadsCompleted += (object source, EventArgs e) =>
			{
				Complete();
			};
			downloadSet.DownloadsFailed += (object source, EventArgs e) =>
			{
				Abort(UpdateError.FailedNetworkResponse);
			};
			downloadSet.OnDownloadUpdate += (object source, EventArgs e) =>
			{
				HttpDownloadProgress downloadProgress = (HttpDownloadProgress)source;
				if (repositoryUpdate != null)
				{
					repositoryUpdate.FireDownloadUpdateEvent(downloadSet, downloadProgress);
				}
			};
			downloadSet.Run();
		}

		private HttpDownload CreateDownload(DataIndexEntry update)
		{
			string downloadUrl = repository.GetRepositoryUrl() + Uri.EscapeUriString(update.filePath);
			string savePath = Path.Combine(dataPath, update.filePath);
			HttpDownload download = new HttpDownload(null, new HttpRequestData(downloadUrl), savePath);
			return download;
		}

		/// <summary>
		/// Get a list of the catalogues and game systems from the data index that need updating.
		/// </summary>
		/// <returns>List of data index entries that require updating</returns>
		public List<DataIndexEntry> GetUpdateDataIndices()
		{
			List<DataIndexEntry> indices = new List<DataIndexEntry>();

			foreach (DataIndexEntry entry in dataIndex.dataIndexEntries)
			{
				if (RequiresUpdate(entry))
				{
					indices.Add(entry);
				}
			}

			return indices;
		}

		/// <summary>
		/// Checks whether the file exists and if it does whether it has an up to date version. If the file is not readable it will also return true.
		/// </summary>
		/// <param name="entry">Entry</param>
		/// <returns>Returns true if it requires updating</returns>
		private bool RequiresUpdate(DataIndexEntry entry)
		{
			string entryPath = Path.Combine(dataPath, entry.filePath);

			if (!File.Exists(entryPath))
				return true;

			// read whole file, could potentialy read as we go but we may as well
			byte[] data = FileUtils.ReadFile(entryPath);
			if (data == null)
				return true;

			DataIndexVersionInfo versionInfo;
			if (entry.dataType == "catalogue")
			{
				versionInfo = DataIndexVersionInfo.GetVersionInfo(data, ".cat", "catalogue");
			}
			else if (entry.dataType == "gamesystem")
			{
				versionInfo = DataIndexVersionInfo.GetVersionInfo(data, ".gst", "gameSystem");
			}
			else
			{
				FrameworkLogger.Error("Unhandled data type: " + entry.dataType);
				return false;
			}

			// if it's out of date or not readable
			if (versionInfo == null || !versionInfo.MatchesRevision(entry))
				return true;

			return false;
		}

		/// <inheritdoc/>
		public override UpdateState GetState()
		{
			return UpdateState.UpdateGameSystem;
		}

		/// <summary>
		/// Optionally store the repository update for keeping track of progress during the update process.
		/// </summary>
		/// <param name="repositoryUpdate">Repository update</param>
		public void SetRepositoryUpdate(RepositoryUpdate repositoryUpdate)
		{
			this.repositoryUpdate = repositoryUpdate;
		}
	}
}
