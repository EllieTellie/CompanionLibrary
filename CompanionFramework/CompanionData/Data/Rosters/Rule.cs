using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Rule : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public string description;

		public string publicationId;
		public string page;
		public bool hidden;

		public List<Modifier> modifiers;
		public List<ModifierGroup> modifierGroups;

		public Rule(XmlNode node) : base(node)
		{
		}

		public string GetName()
		{
			return name;
		}

		public string GetId()
		{
			return id;
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			publicationId = node.GetAttribute("publicationId");
			page = node.GetAttribute("page");
			hidden = node.GetAttributeBool("hidden");

			XmlNode desriptionNode = node.GetNode("description");
			if (desriptionNode != null)
				description = desriptionNode.InnerText;

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			modifierGroups = ParseXmlList<ModifierGroup>(node.GetNodesFromPath("modifierGroups", "modifierGroup"));
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("rule");
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("publicationId", publicationId);
			writer.WriteAttribute("page", page);
			writer.WriteAttribute("hidden", hidden);

			if (description != null) // write empty strings is fine I guess?
			{
				writer.WriteStartElement("description");
				writer.WriteValue(description);
				writer.WriteEndElement();
			}

			WriteXmlList(writer, modifiers, "modifiers");
			WriteXmlList(writer, modifierGroups, "modifierGroups");

			writer.WriteEndElement();
		}
    }
}
