using System.IO;
using System.Text;
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

		/// <summary>
		/// Return the data name minus any characters that are not valid.
		/// </summary>
		/// <returns>Safe data name</returns>
		public string GetDataNameSafe()
		{
			char[] invalidPathChars = Path.GetInvalidFileNameChars();

			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < dataName.Length; i++)
			{
				char c = dataName[i];

				bool validChar = true;
				foreach (char invalidChar in invalidPathChars)
				{
					if (dataName[i] == invalidChar)
					{
						validChar = false;
						break;
					}
				}

				if (validChar)
					builder.Append(c);
			}

			return builder.ToString();
		}
	}
}
