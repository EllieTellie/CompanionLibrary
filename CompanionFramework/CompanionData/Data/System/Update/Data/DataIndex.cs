using CompanionFramework.IO.Utils;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class DataIndex : XmlData
	{
		public string battleScribeVersion;
		public string name;
		public string indexUrl;

		public List<DataIndexEntry> dataIndexEntries;

		// battle scribe has repositoryUrls in here but they are not used so I don't know how to read them

		protected string cachedRepositoryDataUrl;

		public DataIndex(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			battleScribeVersion = node.GetAttribute("battleScribeVersion");
			name = node.GetAttribute("name");
			indexUrl = node.GetAttribute("indexUrl");

			dataIndexEntries = ParseXmlList<DataIndexEntry>(node.GetNodesFromPath("dataIndexEntries", "dataIndexEntry"));
		}

		public static byte[] DecompressDataIndex(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".xml");
		}

		public static DataIndex LoadDataIndexXml(byte[] data)
		{
			// unzip data
			byte[] uncompressedData = DecompressDataIndex(data);

			string xml = FileUtils.GetString(uncompressedData);

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);

			return new DataIndex(xmlDocument.GetNode("dataIndex"));
		}

		public string GetRepositoryDataUrl()
		{
			if (cachedRepositoryDataUrl != null)
				return cachedRepositoryDataUrl;

			cachedRepositoryDataUrl = FileUtils.GetDirectoryFromUri(indexUrl);

			if (!cachedRepositoryDataUrl.EndsWith("/"))
				cachedRepositoryDataUrl += "/";

			return cachedRepositoryDataUrl;
		}
	}
}
