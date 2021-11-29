using System.Xml;

namespace Companion.Data
{
	public class Constraint : XmlData
	{
		public string field;
		public string scope;
		public double value;
		public bool percentValue;
		public bool shared;
		public bool includeChildSelections;
		public bool includeChildForces;
		public string id;
		public string type;

		public Constraint(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			field = node.GetAttribute("field");
			scope = node.GetAttribute("scope");
			value = node.GetAttributeDouble("value");
			percentValue = node.GetAttributeBool("percentValue");
			shared = node.GetAttributeBool("shared");
			includeChildSelections = node.GetAttributeBool("includeChildSelections");
			includeChildForces = node.GetAttributeBool("includeChildForces");
			id = node.GetAttribute("id");
			type = node.GetAttribute("type");
		}
	}
}
