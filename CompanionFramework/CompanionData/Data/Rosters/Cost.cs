using System.Xml;

namespace Companion.Data
{
	public class Cost : XmlData, INameable
	{
		public string name;
		public double value;

		public Cost(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			name = node.GetAttribute("name");
			value = node.GetAttributeDouble("value");
		}

		public string GetName()
		{
			return name;
		}

		public override string ToString()
		{
			return name + ": " + value;
		}
	}
}
