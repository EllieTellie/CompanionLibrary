using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Force : XmlData, INameable
	{
		public string id;
		public string name;
		public string entryId;
		public string catalogueId;
		public string catalogueRevision;
		public string catalogueName;

		public List<Selection> selections;
		public List<Rule> rules;
		public List<Publication> publications;
		public List<Category> categories;

		public Force(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			entryId = node.GetAttribute("entryId");
			catalogueId = node.GetAttribute("catalogueId");
			catalogueRevision = node.GetAttribute("catalogueRevision");
			catalogueName = node.GetAttribute("catalogueName");

			selections = ParseXmlList<Selection>(node.GetNodesFromPath("selections", "selection"));
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"));
			publications = ParseXmlList<Publication>(node.GetNodesFromPath("publications", "publication"));
			categories = ParseXmlList<Category>(node.GetNodesFromPath("categories", "category"));
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("force");
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("entryId", entryId);
			writer.WriteAttribute("catalogueId", catalogueId);
			writer.WriteAttribute("catalogueRevision", catalogueRevision);
			writer.WriteAttribute("catalogueName", catalogueName);

			WriteXmlList(writer, selections, "selections");
			WriteXmlList(writer, rules, "rules");
			WriteXmlList(writer, publications, "publications");
			WriteXmlList(writer, categories, "categories");

			writer.WriteEndElement();
		}

        public void AddSelection(Selection selection)
		{
			if (selections == null)
				selections = new List<Selection>();

			selections.Add(selection);
		}

		public string GetName()
		{
			return name;
		}

		protected override void InitFields()
		{
			base.InitFields();

			selections = new List<Selection>();
			rules = new List<Rule>();

			AddField(selections);
			AddField(rules);
		}

		public override string ToString()
		{
			return name;
		}
	}
}