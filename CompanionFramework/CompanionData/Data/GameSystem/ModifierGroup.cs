using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class ModifierGroup : XmlData
	{
		public List<Modifier> modifiers;
		public List<Condition> conditions;
		public List<ConditionGroup> conditionGroups;
		public List<Repeat> repeats;
		public List<ModifierGroup> modifierGroups;

		public ModifierGroup(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			conditions = ParseXmlList<Condition>(node.GetNodesFromPath("conditions", "condition"));
			conditionGroups = ParseXmlList<ConditionGroup>(node.GetNodesFromPath("conditionGroups", "conditionGroup"));
			repeats = ParseXmlList<Repeat>(node.GetNodesFromPath("repeats", "repeat"));
			modifierGroups = ParseXmlList<ModifierGroup>(node.GetNodesFromPath("modifierGroups", "modifierGroup"));
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("modifierGroup");
			WriteXmlList(writer, modifiers, "modifiers");
			WriteXmlList(writer, conditions, "conditions");
			WriteXmlList(writer, conditionGroups, "conditionGroups");
			WriteXmlList(writer, repeats, "repeats");
			WriteXmlList(writer, modifierGroups, "modifierGroups");
			writer.WriteEndElement();
		}
    }
}
