using System.Xml;

namespace Companion.Data
{
	public class CostLimit : XmlData
	{
		public string name;
		public string typeId;
		public double value;

		public CostLimit(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			name = node.GetAttribute("name");
			typeId = node.GetAttribute("typeId");
			value = node.GetAttributeDouble("value");
		}
	}
}
