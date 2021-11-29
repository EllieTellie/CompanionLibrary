using System.Xml;

namespace Companion.Data
{
	public class CostType : XmlData, IIdentifiable, INameable
	{
		public string id;
		public string name;
		public double defaultCostLimit;
		public bool hidden;

		public CostType(XmlNode node) : base(node)
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
			defaultCostLimit = node.GetAttributeDouble("defaultCostLimit");
			hidden = node.GetAttributeBool("hidden");
		}
	}
}
