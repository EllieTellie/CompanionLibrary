using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class ConditionGroup : XmlData
	{
		public string type;
		public List<Condition> conditions;
		public List<ConditionGroup> conditionGroups;

		public ConditionGroup(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			type = node.GetAttribute("type");
			conditions = ParseXmlList<Condition>(node.GetNodesFromPath("conditions", "condition"));
			conditionGroups = ParseXmlList<ConditionGroup>(node.GetNodesFromPath("conditionGroups", "conditionGroup"));
		}
	}
}
