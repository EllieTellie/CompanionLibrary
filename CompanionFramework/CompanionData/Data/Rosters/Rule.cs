using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Rule : XmlData, INameable
	{
		public string name;
		public string description;

		public List<Modifier> modifiers;
		public List<ModifierGroup> modifierGroups;

		public Rule(XmlNode node) : base(node)
		{
		}

		public string GetName()
		{
			return name;
		}

		protected override void OnParseNode()
		{
			name = node.GetAttribute("name");

			XmlNode desriptionNode = node.GetNode("description");
			if (desriptionNode != null)
				description = desriptionNode.InnerText;

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			modifierGroups = ParseXmlList<ModifierGroup>(node.GetNodesFromPath("modifierGroups", "modifierGroup"));
		}
	}
}
