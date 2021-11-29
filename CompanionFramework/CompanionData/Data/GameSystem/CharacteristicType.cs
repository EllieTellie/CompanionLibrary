using System.Xml;

namespace Companion.Data
{
	public class CharacteristicType : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;

		public CharacteristicType(XmlNode node) : base(node)
		{
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
		}
	}
}
