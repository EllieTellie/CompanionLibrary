using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Profile : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public string typeName;
		public string publicationId;
		public string page;
		public string typeId;
		public bool hidden;

		public List<Characteristic> characteristics;
		public List<Modifier> modifiers;

		public Profile(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			typeName = node.GetAttribute("typeName");
			publicationId = node.GetAttribute("publicationId");
			page = node.GetAttribute("page");
			typeId = node.GetAttribute("typeId");
			hidden = node.GetAttributeBool("hidden");

			characteristics = ParseXmlList<Characteristic>(node.GetNodesFromPath("characteristics", "characteristic"));
			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("profile");
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("publicationId", publicationId);
			writer.WriteAttribute("page", page);
			writer.WriteAttribute("hidden", hidden);
			writer.WriteAttribute("typeId", typeId);
			writer.WriteAttribute("typeName", typeName);

			WriteXmlList(writer, characteristics, "characteristics");
			WriteXmlList(writer, modifiers, "modifiers");

			writer.WriteEndElement();
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