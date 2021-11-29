﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Modifier : XmlData
	{
		public string type;
		public string field;
		public string value;

		public List<ConditionGroup> conditionGroups;
		public List<Condition> conditions;
		public List<Repeat> repeats;

		public Modifier(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			type = node.GetAttribute("type");
			field = node.GetAttribute("field");
			value = node.GetAttribute("value");

			conditionGroups = ParseXmlList<ConditionGroup>(node.GetNodesFromPath("conditionGroups", "conditionGroup"));
			conditions = ParseXmlList<Condition>(node.GetNodesFromPath("conditions", "condition"));
			repeats = ParseXmlList<Repeat>(node.GetNodesFromPath("repeats", "repeat"));
		}
	}
}
