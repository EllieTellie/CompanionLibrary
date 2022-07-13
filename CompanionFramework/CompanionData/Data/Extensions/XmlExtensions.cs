using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Companion.Data
{
	public static class XmlExtensions
	{

		/// <summary>
		/// Get an attribute value with the specified name and attempt to convert it to an integer.
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="name">Name</param>
		/// <param name="defaultValue">Default value if it is not found or fails to parse</param>
		/// <returns>Attribute value</returns>
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

		/// <summary>
		/// Get an attribute value with the specified name and attempt to convert it to a double.
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="name">Name</param>
		/// <param name="defaultValue">Default value if it is not found or fails to parse</param>
		/// <returns>Attribute value</returns>
		public static double GetAttributeDouble(this XmlNode node, string name, double defaultValue = 0)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			if (result == null)
				return defaultValue;

			string value = result != null ? result.Value : null;

			double doubleValue;
			if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
            {
				return doubleValue;
            }
			else
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Get an attribute value with the specified name and attempt to convert it to an integer.
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="name">Name</param>
		/// <param name="defaultValue">Default value if it is not found or fails to parse</param>
		/// <returns>Attribute value</returns>
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

		/// <summary>
		/// Get an attribute value with the specified name.
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="name">Name</param>
		/// <returns>Attribute value</returns>
		public static string GetAttribute(this XmlNode node, string name)
		{
			XmlNode result = node.Attributes.GetNamedItem(name);

			return result != null ? result.Value : null;
		}

		/// <summary>
		/// Get the nodes by name recursively through all child nodes.
		/// </summary>
		/// <param name="element">Current node</param>
		/// <param name="name">Name</param>
		/// <returns>List of nodes found</returns>
		public static List<XmlNode> GetNodes(this XmlNode element, string name)
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

		/// <summary>
		/// Get a list of child nodes from the provided node. Always returns at least an empty list.
		/// </summary>
		/// <param name="element">Node</param>
		/// <param name="name">Name</param>
		/// <returns>List of child nodes</returns>
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

		/// <summary>
		/// Get a list of nodes from the path provided.
		/// </summary>
		/// <param name="element">Element to search from</param>
		/// <param name="names">Names of the path to found</param>
		/// <returns>List of nodes found or empty list if not found</returns>
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

		/// <summary>
		/// Get a single node by name. If the provided node has that name it will return that node otherwise it will attempt to find it in the children recursively.
		/// </summary>
		/// <param name="element">Node</param>
		/// <param name="name">Name</param>
		/// <returns>Node or null if not found</returns>
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

		/// <summary>
		/// Write a string value out to the writer.
		/// </summary>
		/// <param name="writer">Xml Writer</param>
		/// <param name="localName">Local name</param>
		/// <param name="value">Value</param>
		public static void WriteAttribute(this XmlWriter writer, string localName, string value)
		{
			// ignore empty values
			if (value == null)
            {
				return;
            }

			// didn't work lol because & > &amp; which is ironic because this is just aimed for compatibility with BattleScribe
			//value = value.Replace("\"", "&quot;");
			//value = value.Replace("'", "&apos;");

			writer.WriteAttributeString(localName, value);
		}

		/// <summary>
		/// Write a boolean value out to the writer.
		/// </summary>
		/// <param name="writer">Xml Writer</param>
		/// <param name="localName">Local name</param>
		/// <param name="value">Value</param>
		public static void WriteAttribute(this XmlWriter writer, string localName, bool value)
        {
            writer.WriteAttributeString(localName, value.ToString()); // sometimes BattleScribe uses True and sometimes it uses true, hopefully they are both compatible or else we are doomed.
        }

		/// <summary>
		/// Write an integer value out to the writer.
		/// </summary>
		/// <param name="writer">Xml Writer</param>
		/// <param name="localName">Local name</param>
		/// <param name="value">Value</param>
		public static void WriteAttribute(this XmlWriter writer, string localName, int value)
		{
			writer.WriteAttributeString(localName, value.ToString());
		}

		/// <summary>
		/// Write a double value out to the writer. This uses CultureInfo.InvariantCulture and aims to write as compatible as possible with battle scribe by alway writing a decimal place.
		/// </summary>
		/// <param name="writer">Xml Writer</param>
		/// <param name="localName">Local name</param>
		/// <param name="value">Value</param>
		public static void WriteAttribute(this XmlWriter writer, string localName, double value)
        {
			// slightly meh formatting wise but supposedly the easiest way to enforce one decimal place to keep it compatible with battle scribe
			string text = value % 1 == 0 ? value.ToString("0.0", CultureInfo.InvariantCulture) : value.ToString(CultureInfo.InvariantCulture);
			writer.WriteAttributeString(localName, text);
		}
	}
}