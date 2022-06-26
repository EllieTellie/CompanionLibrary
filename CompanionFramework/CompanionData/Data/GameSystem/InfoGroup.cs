using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class InfoGroup : XmlData
	{
		public string id;
		public string name;
		public bool hidden;

		public List<Modifier> modifiers;
		public List<Profile> profiles;
		public List<InfoLink> infoLinks;
		public List<Rule> rules;

		public InfoGroup(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			profiles = ParseXmlList<Profile>(node.GetNodesFromPath("profiles", "profile"));
			infoLinks = ParseXmlList<InfoLink>(node.GetNodesFromPath("infoLinks", "infoLink"));
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"));
		}
	}
}
