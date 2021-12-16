using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Companion.Data.Utils
{
	public static class XmlUtils
	{
		public static T ParseXml<T>(XmlNode node) where T : XmlData
		{
			return XmlDataFactory.Instance.Create<T>(node);
		}

		public static List<T> ParseXmlList<T>(List<XmlNode> nodes, IRootContainer rootContainer = null) where T : XmlData
		{
			if (nodes == null)
				return new List<T>();

			List<T> results = new List<T>(nodes.Count);
			foreach (XmlNode node in nodes)
			{
				T instance = XmlDataFactory.Instance.Create<T>(node);

				if (rootContainer != null)
					instance.SetRootContainer(rootContainer);

				results.Add(instance);
			}

			return results;
		}

		public static int GetAttributeInt(XmlNode node, string name)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			if (result == null)
				return 0;

			string value = result != null ? result.Value : null;

			int intValue;
			int.TryParse(value, out intValue);

			return intValue;
		}

		public static string GetPath(XmlNode node)
		{
			StringBuilder builder = new StringBuilder();
			GetPath(builder, node);
			return builder.ToString();
		}

		private static void GetPath(StringBuilder builder, XmlNode node)
		{
			if (node.ParentNode != null)
			{
				GetPath(builder, node.ParentNode);
			}

			if (builder.Length > 0)
			{
				builder.Append("/");
			}

			builder.Append(node.Name);
		}

		public static string GetAttribute(XmlNode node, string name)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			return result != null ? result.Value : null;
		}

		public static List<XmlNode> GetNodes(XmlNode element, string name)
		{
			List<XmlNode> nodes = new List<XmlNode>();
			GetNodes(element, nodes, name);
			return nodes;
		}

		public static void GetNodes(XmlNode element, List<XmlNode> nodes, string name)
		{
			if (element == null)
			{
				return;
			}
			else if (element.Name == name)
			{
				nodes.Add(element);
				return;
			}
			else
			{
				if (!element.HasChildNodes)
					return;

				foreach (XmlNode node in element.ChildNodes)
				{
					GetNodes(node, nodes, name);
				}
			}
		}

		public static List<XmlNode> GetChildNodes(XmlNode element, string name)
		{
			List<XmlNode> nodes = new List<XmlNode>();
			if (!element.HasChildNodes)
				return nodes;

			foreach (XmlNode node in element.ChildNodes)
			{
				if (node.Name == name)
					nodes.Add(node);
			}

			return nodes;
		}

		public static List<XmlNode> GetNodesFromPath(XmlNode element, params string[] names)
		{
			List<XmlNode> searchNodes = new List<XmlNode>();
			searchNodes.Add(element);
			for (int i = 0; i < names.Length; i++)
			{
				string name = names[i];

				List<XmlNode> validNodes = new List<XmlNode>();
				foreach (XmlNode search in searchNodes)
				{
					searchNodes = GetChildNodes(search, name);

					if (searchNodes != null)
					{
						// add them all
						foreach (XmlNode result in searchNodes)
						{
							validNodes.Add(result);
						}
					}
				}

				if (i == names.Length - 1)
				{
					return searchNodes;
				}
				else
				{
					searchNodes = validNodes; // search next depth
				}
			}

			return new List<XmlNode>(); // failed
		}

		public static XmlNode GetNode(XmlNode element, string name)
		{
			if (element == null)
			{
				return null;
			}
			else if (element.Name == name)
			{
				return element;
			}
			else
			{
				if (!element.HasChildNodes)
					return null;

				foreach (XmlNode node in element.ChildNodes)
				{
					XmlNode foundNode = GetNode(node, name);
					if (foundNode != null)
						return foundNode;
				}

				return null;
			}
		}
	}
}
