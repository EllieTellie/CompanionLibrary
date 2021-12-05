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
			if (!Directory.Exists(dataPath))
				Directory.CreateDirectory(dataPath);

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

			HttpDownloadSet downloadSet = new HttpDownloadSet(downloads.ToArray());
			downloadSet.DownloadsCompleted += (object source, EventArgs e) =>
			{
				Complete();
			};
			downloadSet.DownloadsFailed += (object source, EventArgs e) =>
			{
				Abort(UpdateError.FailedNetworkResponse);
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
				versionInfo = GetVersionInfo(data, ".cat", "catalogue");
			}
			else if (entry.dataType == "gamesystem")
			{
				versionInfo = GetVersionInfo(data, ".gst", "gameSystem");
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

		/// <summary>
		/// Get the version information from the compressed data file on disk.
		/// </summary>
		/// <param name="data">Byte data</param>
		/// <param name="fileExtension">The extension of the compressed file starting with a period</param>
		/// <param name="elementName">The name of the first element in the xml file that needs to match</param>
		/// <returns></returns>
		private DataIndexVersionInfo GetVersionInfo(byte[] data, string fileExtension, string elementName)
		{
			byte[] uncompressedData = CompressionUtils.DecompressFileFromZip(data, fileExtension);
			string text = FileUtils.GetString(uncompressedData); // could read partial, but for now just reading the full file
			using (StringReader textReader = new StringReader(text))
			{
				using (XmlReader reader = XmlReader.Create(textReader))
				{
					// read the element with the version information
					if (reader.ReadToFollowing(elementName))
					{
						// this should be the start element always
						if (reader.IsStartElement())
						{
							// these should match between catalogue and gamesystem so just read these only
							string id = reader.GetAttribute("id");
							string name = reader.GetAttribute("name");
							string revision = reader.GetAttribute("revision");
							string battleScribeVersion = reader.GetAttribute("battleScribeVersion");

							return new DataIndexVersionInfo(id, name, revision, battleScribeVersion);
						}
					}
					else
					{
						return null;
					}
				}
			}

			return null;
		}

		/// <inheritdoc/>
		public override UpdateState GetState()
		{
			return UpdateState.UpdateGameSystem;
		}
	}
}
