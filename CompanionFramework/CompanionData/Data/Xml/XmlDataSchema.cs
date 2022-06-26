using System;
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

        public XmlDataSchemaNode GetNode(string name, List<XmlDataSchemaNode> inputList)
        {
            foreach (XmlDataSchemaNode node in inputList)
            {
                if (node.name == name)
                    return node;
            }

            return null;
        }

        public List<XmlDataSchemaNode> Flatten()
        {
            List<XmlDataSchemaNode> flattened = new List<XmlDataSchemaNode>();

            Flatten(flattened, nodeList);

            return flattened;
        }

        private void Flatten(List<XmlDataSchemaNode> flattened, List<XmlDataSchemaNode> nodeList)
        {
            foreach (XmlDataSchemaNode node in nodeList)
            {
                if (node.name == "#text") // ignore text nodes
                    continue;

                XmlDataSchemaNode existingNode = GetNode(node.name, flattened);
                if (existingNode != null)
                    existingNode.Merge(node);
                else
                    flattened.Add(new XmlDataSchemaNode(node)); // copy to a simple version

                // recursive
                Flatten(flattened, node.nodeList);
            }
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
