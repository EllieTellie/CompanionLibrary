using System.Xml;

namespace Companion.Data
{
	public class Characteristic : XmlData, INameable
	{
		public string name;
		public string typeId;
		public string value;

		public Characteristic(XmlNode node) : base(node)
		{
		}

		public string GetName()
		{
			return name;
		}

		protected override void OnParseNode()
		{
			name = node.GetAttribute("name");
			typeId = node.GetAttribute("typeId");
			value = node.InnerText;
		}
	}
}
