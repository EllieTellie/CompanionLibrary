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

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("characteristic");
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("typeId", typeId);
			if (!string.IsNullOrEmpty(value)) // just write /> instead of no text
				writer.WriteValue(value);
			writer.WriteEndElement();
        }
    }
}
