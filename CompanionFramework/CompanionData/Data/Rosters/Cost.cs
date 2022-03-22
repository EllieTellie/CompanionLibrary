using System.Globalization;
using System.Xml;

namespace Companion.Data
{
	public class Cost : XmlData, INameable
	{
		public string name;
		public string typeId;
		public double value;

		public Cost(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			name = node.GetAttribute("name");
			typeId = node.GetAttribute("typeId");
			value = node.GetAttributeDouble("value");	
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("cost");
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("typeId", typeId);
			writer.WriteAttribute("value", value);
			writer.WriteEndElement();
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
