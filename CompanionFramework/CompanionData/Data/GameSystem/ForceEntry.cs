using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class ForceEntry : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public bool hidden;

		public List<Modifier> modifiers;
		public List<ForceEntry> forceEntries;
		public List<CategoryLink> categoryLinks;
		public List<Constraint> constraints;
		public List<Rule> rules;

		public ForceEntry(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			forceEntries = ParseXmlList<ForceEntry>(node.GetNodesFromPath("forceEntries", "forceEntry"));
			categoryLinks = ParseXmlList<CategoryLink>(node.GetNodesFromPath("categoryLinks", "categoryLink"));
			constraints = ParseXmlList<Constraint>(node.GetNodesFromPath("constraints", "constraint"));
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"));
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		protected override bool HasNameOverride(string name)
		{
			Modifier modifier = GetModifierForName(name);
			return modifier != null;
		}

		public Modifier GetModifierByField(string field)
		{
			foreach (Modifier modifier in modifiers)
			{
				if (modifier.field == field)
					return modifier;
			}

			return null;
		}

		public Modifier GetModifierForName(string name)
		{
			foreach (Modifier modifier in modifiers)
			{
				if (modifier.field == "name" && modifier.type == "set" && modifier.value == name)
				{
					return modifier;
				}
			}

			return null;
		}
	}
}
