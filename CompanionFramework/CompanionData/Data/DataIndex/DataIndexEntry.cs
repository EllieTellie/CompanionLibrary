using System.Xml;

namespace Companion.Data
{
	public class DataIndexEntry : XmlData
	{
		public string filePath;
		public string dataType;
		public string dataId;
		public string dataName;
		public string dataBattleScribeVersion;
		public string dataRevision;

		public DataIndexEntry(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			filePath = node.GetAttribute("filePath");
			dataType = node.GetAttribute("dataType");
			dataId = node.GetAttribute("dataId");
			dataName = node.GetAttribute("dataName");
			dataBattleScribeVersion = node.GetAttribute("dataBattleScribeVersion");
			dataRevision = node.GetAttribute("dataRevision");
		}

		public int GetRevision()
		{
			if (int.TryParse(dataRevision, out int parsed))
			{
				return parsed;
			}
			else
			{
				return -1;
			}
		}
	}
}
