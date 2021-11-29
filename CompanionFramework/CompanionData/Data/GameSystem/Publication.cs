using System.Xml;

namespace Companion.Data
{
	public class Publication : XmlData, IIdentifiable, INameable
	{
		public string id;
		public string name;
		public string shortName;
		public string publisher;
		public string publicationDate;
		public string publisherUrl;

		public Publication(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			shortName = node.GetAttribute("shortName");
			publisher = node.GetAttribute("publisher");
			publicationDate = node.GetAttribute("publicationDate");
			publisherUrl = node.GetAttribute("publisherUrl");
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}
	}
}
