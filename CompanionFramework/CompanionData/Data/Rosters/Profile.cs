using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Profile : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public string typeName;

		public List<Characteristic> characteristics;

		public Profile(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			typeName = node.GetAttribute("typeName");

			characteristics = ParseXmlList<Characteristic>(node.GetNodesFromPath("characteristics", "characteristic"));
		}

		public Characteristic GetCharacteristic(string name)
		{
			foreach (Characteristic characteristic in characteristics)
			{
				if (characteristic.name == name)
					return characteristic;
			}

			return null;
		}

		public string GetName()
		{
			return name;
		}

		public string GetId()
		{
			return id;
		}

		protected override void InitFields()
		{
			base.InitFields();

			characteristics = new List<Characteristic>();
		}
	}
}