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

        /// <summary>
        /// Get the node, this can be null if not from an XmlNode. Mostly for debugging.
        /// </summary>
        public XmlNode Node { get { return node; } }
        protected XmlNode node;

        public XmlDataSchemaNode(string name) // for empty node
        {
            this.name = name;
        }

        public XmlDataSchemaNode(XmlNode node)
        {
            this.name = node.Name;
            this.node = node;

            Append(node);
        }

        internal XmlDataSchemaNode(XmlDataSchemaNode node)
        {
            this.name = node.name;
            attributeNames.AddRange(node.attributeNames);

            foreach (XmlDataSchemaNode child in node.nodeList)
            {
                // just simplify this as we are trying to flatten this
                nodeList.Add(new XmlDataSchemaNode(child.name));
            }
        }

        public void Merge(XmlDataSchemaNode node)
        {
            foreach (string attributeName in node.attributeNames)
            {
                if (!attributeNames.Contains(attributeName))
                    attributeNames.Add(attributeName);
            }

            foreach (XmlDataSchemaNode child in node.nodeList)
            {
                XmlDataSchemaNode existingNode = GetNode(child.name);
                if (existingNode == null)
                    nodeList.Add(new XmlDataSchemaNode(child.name)); // simplify
            }
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

        public void Print(StringBuilder builder, int indent = 1)
        {
            for (int i = 0; i < indent - 1; i++)
                builder.Append('\t');

            if (name == "#text")
                builder.AppendLine("Text");
            else
                builder.AppendLine("Node: " + name);

            foreach (string attribute in attributeNames)
            {                                                                 
                for (int i=0; i<indent; i++)
                    builder.Append('\t');

                builder.AppendLine("Attribute: " + attribute);
            }

            int newIndent = indent + 1;
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
