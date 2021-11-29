using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public static class XmlExtensions
	{
		public static int GetAttributeInt(this XmlNode node, string name, int defaultValue = 0)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			if (result == null)
				return defaultValue;

			string value = result != null ? result.Value : null;

			int intValue;
			if (int.TryParse(value, out intValue))
			{
				return intValue;
			}
			else
			{
				return defaultValue;
			}
		}

		public static double GetAttributeDouble(this XmlNode node, string name)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			if (result == null)
				return 0.0;

			string value = result != null ? result.Value : null;

			double intValue;
			double.TryParse(value, out intValue);

			return intValue;
		}

		public static bool GetAttributeBool(this XmlNode node, string name, bool defaultValue = false)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			if (result == null)
				return defaultValue;

			string value = result.Value;

			if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				return true;
			else if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
				return false;
			else
				return defaultValue;
		}

		public static string GetAttribute(this XmlNode node, string name)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			return result != null ? result.Value : null;
		}

		public static List<XmlNode> GetNodes(this XmlNode element, string name)
		{
			List<XmlNode> nodes = new List<XmlNode>();
			GetNodes(element, nodes, name);
			return nodes;
		}

		public static void GetNodes(this XmlNode element, List<XmlNode> nodes, string name)
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

		public static List<XmlNode> GetChildNodes(this XmlNode element, string name)
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

		public static List<XmlNode> GetNodesFromPath(this XmlNode element, params string[] names)
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

		public static XmlNode GetNode(this XmlNode element, string name)
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