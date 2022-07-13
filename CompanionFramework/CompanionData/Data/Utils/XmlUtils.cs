using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Companion.Data.Utils
{
	public static class XmlUtils
	{
		/// <summary>
		/// Parse a single XmlNode into an XmlData class.
		/// </summary>
		/// <typeparam name="T">Type of data</typeparam>
		/// <param name="node">Node to parse</param>
		/// <returns>Parsed XmlData</returns>
		public static T ParseXml<T>(XmlNode node) where T : XmlData
		{
			return XmlDataFactory.Instance.Create<T>(node);
		}

		/// <summary>
		/// Parse a list of XmlNodes into a list of XmlData. If a root container is provided it is set on every data in the list (for example a Catalogue/GameSystem root container). Always returns at least an empty list.
		/// </summary>
		/// <typeparam name="T">Type of data</typeparam>
		/// <param name="nodes">Nodes to parse</param>
		/// <param name="rootContainer">Optional root container</param>
		/// <returns>List of XmlData</returns>
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

		/// <summary>
		/// Construct a string that has the node path for debugging.
		/// </summary>
		/// <param name="node">Node</param>
		/// <returns>Path to node</returns>
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

		/// <summary>
		/// Get an attribute value with the specified name.
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="name">Name</param>
		/// <returns>Attribute value</returns>
		public static string GetAttribute(XmlNode node, string name)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			return result != null ? result.Value : null;
		}

		/// <summary>
		/// Get an attribute value with the specified name and attempt to convert it to an integer.
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="name">Name</param>
		/// <returns>Attribute value</returns>
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

		/// <summary>
		/// Get the nodes by name recursively through all child nodes.
		/// </summary>
		/// <param name="element">Current node</param>
		/// <param name="name">Name</param>
		/// <returns>List of nodes found</returns>
		public static List<XmlNode> GetNodes(XmlNode element, string name)
		{
			List<XmlNode> nodes = new List<XmlNode>();
			GetNodes(element, nodes, name);
			return nodes;
		}

		/// <summary>
		/// Get the nodes by name recursively through all child nodes and add it to the list of nodes provided.
		/// </summary>
		/// <param name="element">Current node</param>
		/// <param name="nodes">List to add results to</param>
		/// <param name="name">Name</param>
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

		/// <summary>
		/// Get a list of child nodes from the provided node. Always returns at least an empty list.
		/// </summary>
		/// <param name="element">Node</param>
		/// <param name="name">Name</param>
		/// <returns>List of child nodes</returns>
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

		/// <summary>
		/// Get a list of nodes from the path provided.
		/// </summary>
		/// <param name="element">Element to search from</param>
		/// <param name="names">Names of the path to found</param>
		/// <returns>List of nodes found or empty list if not found</returns>
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

		/// <summary>
		/// Get a single node by name. If the provided node has that name it will return that node otherwise it will attempt to find it in the children recursively.
		/// </summary>
		/// <param name="element">Node</param>
		/// <param name="name">Name</param>
		/// <returns>Node or null if not found</returns>
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
