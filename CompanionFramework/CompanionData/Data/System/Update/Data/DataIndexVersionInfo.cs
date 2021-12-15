using CompanionFramework.IO.Utils;
using System;
using System.IO;
using System.Xml;

namespace Companion.Data.System.Update
{
	public class DataIndexVersionInfo
	{
		public string id;
		public string name;
		public string revision;
		public string battleScribeVersion;

		public DataIndexVersionInfo(string id, string name, string revision, string battleScribeVersion)
		{
			this.id = id;
			this.name = name;
			this.revision = revision;
			this.battleScribeVersion = battleScribeVersion;
		}

		public bool MatchesRevision(DataIndexEntry dataIndex)
		{
			// validate id
			if (dataIndex.dataId != id)
				return false;

			// validate revision
			if (dataIndex.dataRevision != revision)
			{
				if (GetRevision() < dataIndex.GetRevision())
					return false;
			}

			return true;
		}

		public int GetRevision()
		{
			if (int.TryParse(revision, out int parsed))
			{
				return parsed;
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// Get the version information from the compressed data file on disk.
		/// </summary>
		/// <param name="data">Byte data</param>
		/// <param name="fileExtension">The extension of the compressed file starting with a period</param>
		/// <param name="elementName">The name of the first element in the xml file that needs to match</param>
		/// <returns>Version information</returns>
		public static DataIndexVersionInfo GetVersionInfo(byte[] data, string fileExtension, string elementName)
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
	}
}
