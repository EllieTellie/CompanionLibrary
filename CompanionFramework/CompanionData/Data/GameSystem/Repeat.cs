using System.Xml;

namespace Companion.Data
{
	public class Repeat : XmlData
	{
		public string field;
		public string scope;
		public double value;
		public bool percentValue;
		public bool shared;
		public bool includeChildSelections;
		public bool includeChildForces;
		public string childId;
		public int repeats;
		public bool roundUp;

		public Repeat(XmlNode node) : base(node)
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
			childId = node.GetAttribute("childId");
			repeats = node.GetAttributeInt("repeats");
			roundUp = node.GetAttributeBool("roundUp");
		}
	}
}
