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
	public class UpdateGameSystemProcess : CoreUpdateProcess
	{
		protected readonly RepositoryIndex repositoryIndex;
		protected readonly DataIndex dataIndex;
		protected readonly string dataPath;

		public UpdateGameSystemProcess(RepositoryIndex repositoryIndex, DataIndex dataIndex, string dataPath)
		{
			this.repositoryIndex = repositoryIndex;
			this.dataIndex = dataIndex;
			this.dataPath = dataPath;
		}

		public override void Execute(UpdateStateData state)
		{
			if (repositoryIndex == null)
			{
				FrameworkLogger.Error("Missing repository index");
				Abort();
				return;
			}
			else if (dataIndex == null)
			{
				FrameworkLogger.Error("Missing data index");
				Abort();
				return;
			}
			else if (dataPath == null)
			{
				FrameworkLogger.Error("Missing path");
				Abort();
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
				download.SetAsync(false); // for debugging
				if (download != null) // should never be null most likely
					downloads.Add(download);
			}

			HttpDownloadSet downloadSet = new HttpDownloadSet(downloads.ToArray());
			downloadSet.Run();
		}

		private HttpDownload CreateDownload(DataIndexEntry update)
		{
			string downloadUrl = dataIndex.GetRepositoryDataUrl() + Uri.EscapeUriString(update.filePath);
			string savePath = Path.Combine(dataPath, update.filePath);
			HttpDownload download = new HttpDownload(null, new HttpRequestData(downloadUrl), savePath);
			return download;
		}

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

			// if it's out of date
			if (!versionInfo.MatchesRevision(entry))
				return true;

			return false;
		}

		private DataIndexVersionInfo GetVersionInfo(byte[] data, string fileExtension, string elementName)
		{
			byte[] uncompressedData = CompressionUtils.DecompressFileFromZip(data, fileExtension);
			string text = FileUtils.GetString(uncompressedData); // could read partial, but for now just reading full
			using (StringReader textReader = new StringReader(text))
			{
				using (XmlReader reader = XmlReader.Create(textReader))
				{
					if (reader.ReadToFollowing(elementName))
					{
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

		public override UpdateState GetState()
		{
			return UpdateState.UpdateGameSystem;
		}
	}
}
