using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class CategoryEntry : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public bool hidden;

		public string publicationId;
		public string page;

		public List<Modifier> modifiers;
		public List<Constraint> constraints;
		public List<InfoLink> infoLinks;

		public CategoryEntry(XmlNode node) : base(node)
		{
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			constraints = ParseXmlList<Constraint>(node.GetNodesFromPath("constraints", "constraint"));
			infoLinks = ParseXmlList<InfoLink>(node.GetNodesFromPath("infoLinks", "infoLink"));
		}
	}
}
