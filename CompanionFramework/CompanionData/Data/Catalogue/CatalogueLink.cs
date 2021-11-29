using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class CatalogueLink : XmlData, IIdentifiable, INameable
	{
		public string id;
		public string name;
		public string targetId;
		public string type;
		public bool importRootEntries;

		public CatalogueLink(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			targetId = node.GetAttribute("targetId");
			type = node.GetAttribute("type");
			importRootEntries = node.GetAttributeBool("importRootEntries");
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}
	}
}
