using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Companion.Data.Xml
{
    public class XmlDataSchemaNode
    {
        public readonly string name;
        public List<string> attributeNames = new List<string>();

        public List<XmlDataSchemaNode> nodeList = new List<XmlDataSchemaNode>();

        public XmlDataSchemaNode(XmlNode node)
        {
            this.name = node.Name;

            Append(node);
        }

        public void Append(XmlNode node)
        {
            if (node.Attributes != null) // can it be null?
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (!attributeNames.Contains(attribute.Name))
                        attributeNames.Add(attribute.Name);
                }
            }

            if (node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    AddNode(child);
                }
            }
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

        public void Print(StringBuilder builder, int indent = 0)
        {
            for (int i = 0; i < indent; i++)
                builder.Append('\t');

            builder.AppendLine(name);

            foreach (string attribute in attributeNames)
            {                                                                 
                for (int i=0; i<indent; i++)
                    builder.Append('\t');

                builder.AppendLine("Attribute: " + attribute);
            }

            int newIndent = indent++;
            foreach (XmlDataSchemaNode node in nodeList)
            {
                node.Print(builder, newIndent);
            }
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
    }
}
