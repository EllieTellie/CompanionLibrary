using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Companion.Data.Xml
{
    /// <summary>
    /// Class to gather information about single or multiple xml documents.
    /// </summary>
    public class XmlDataSchema
    {
        public List<XmlDataSchemaNode> nodeList = new List<XmlDataSchemaNode>();

        public void ParseDocument(XmlDocument document, bool clearSchema = false)
        {
            if (clearSchema)
                nodeList.Clear();

            AddNode(document);
        }

        public void AddNode(XmlNode node)
        {
            if (node == null)
                return;

            XmlDataSchemaNode existingNode = GetNode(node.Name);
            if (existingNode != null)
                existingNode.Append(node);
            else
                nodeList.Add(new XmlDataSchemaNode(node));
        }

        public XmlDataSchemaNode GetNode(string name)
        {
            foreach (XmlDataSchemaNode node in nodeList)
            {
                if (node.name == name)
                    return node;
            }

            return null;
        }

        public string Print()
        {
            StringBuilder builder = new StringBuilder();
            foreach (XmlDataSchemaNode node in nodeList)
            {
                node.Print(builder);
            }
            return builder.ToString();
        }
    }
}
