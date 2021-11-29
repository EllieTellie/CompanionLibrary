using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class ProfileType : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;

		public List<CharacteristicType> characteristicTypes;

		public ProfileType(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");

			characteristicTypes = ParseXmlList<CharacteristicType>(node.GetNodesFromPath("characteristicTypes", "characteristicType"));
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
