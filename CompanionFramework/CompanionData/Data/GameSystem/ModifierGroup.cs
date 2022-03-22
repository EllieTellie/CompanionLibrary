using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class ModifierGroup : XmlData
	{
		public List<Modifier> modifiers;
		public List<Condition> conditions;

		public ModifierGroup(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			conditions = ParseXmlList<Condition>(node.GetNodesFromPath("conditions", "condition"));
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("modifierGroup");
			WriteXmlList(writer, modifiers, "modifiers");
			WriteXmlList(writer, conditions, "conditions");
			writer.WriteEndElement();
		}
    }
}
