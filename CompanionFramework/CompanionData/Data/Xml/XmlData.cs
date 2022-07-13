using Companion.Data.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data // make it .Xml for the namespace
{
	public abstract class XmlData
	{
		/// <summary>
		/// Optional comment for data.
		/// </summary>
		public string comment;

		/// <summary>
		/// Optional xml schema.
		/// </summary>
		public string xmlns;

		protected readonly XmlNode node;

		/// <summary>
		/// Optional root container.
		/// </summary>
		protected IRootContainer rootContainer;

		/// <summary>
		/// List of xml lists containing all the nodes after parsing.
		/// </summary>
		protected List<IList> fields = new List<IList>();

		/// <summary>
		/// Create a new data class from a node. This parses the node automatically.
		/// </summary>
		/// <param name="node">Node to parse</param>
		public XmlData(XmlNode node)
		{
			this.node = node;

			// helper to just instantly set root container
			if (this is IRootContainer container)
			{
				this.rootContainer = container;
			}

			if (node != null)
			{
				OnParseNode();

				OnParseGlobalData();
			}
			else
			{
				InitFields();
			}

			// automatically add ourselves
			if (this == rootContainer && this is IIdentifiable identifiable)
				rootContainer.AddIdLookup(identifiable);
		}

		/// <summary>
		/// Init the fields usually lists to a new empty list. By default does nothing.
		/// </summary>
		protected virtual void InitFields()
		{
		}

		protected void AddField<T>(IList<T> field) where T : XmlData
		{
			// ignore null lists
			if (field == null)
				return;

			fields.Add((IList)field);
		}

		/// <summary>
		/// Parse any global values that might be present in any nodes in the xml, for example: xmlns and comment.
		/// </summary>
		protected virtual void OnParseGlobalData()
		{
			XmlNode commentNode = node.GetNode("comment");
			if (commentNode != null)
				comment = commentNode.InnerText;

			xmlns = node.GetAttribute("xmlns");
		}

		/// <summary>
		/// Parse the node into data.
		/// </summary>
		protected abstract void OnParseNode();

		/// <summary>
		/// Parse a single XmlNode into an XmlData class.
		/// </summary>
		/// <typeparam name="T">Type of data</typeparam>
		/// <param name="node">Node to parse</param>
		/// <returns>Parsed XmlData</returns>
		public T ParseXml<T>(XmlNode node) where T : XmlData
		{
			return XmlUtils.ParseXml<T>(node);
		}

		/// <summary>
		/// Parse a list of XmlNodes into a list of XmlData and store it in the fields list. If a root container is provided it is set on every data in the list (for example a Catalogue/GameSystem root container). Always returns at least an empty list.
		/// </summary>
		/// <typeparam name="T">Type of data</typeparam>
		/// <param name="nodes">Nodes to parse</param>
		/// <param name="rootContainer">Optional root container</param>
		/// <returns>List of XmlData</returns>
		public List<T> ParseXmlList<T>(List<XmlNode> nodes, IRootContainer rootContainer = null) where T : XmlData
		{
			List<T> results = XmlUtils.ParseXmlList<T>(nodes, rootContainer);
			AddField(results); // automatically add
			return results;
		}

		/// <summary>
		/// Write Xml to the writer. This must be implemented explicitly and by default throws an exception.
		/// </summary>
		/// <param name="writer">Writer to write to</param>
		/// <exception cref="NotImplementedException"></exception>
		public virtual void WriteXml(XmlWriter writer)
        {
			throw new NotImplementedException();
        }

		/// <summary>
		/// Set the root container.
		/// </summary>
		/// <param name="rootContainer">Root container</param>
		public void SetRootContainer(IRootContainer rootContainer)
		{
			this.rootContainer = rootContainer;

			if (rootContainer != null && this is IIdentifiable identifiable)
				rootContainer.AddIdLookup(identifiable);
		}

		/// <summary>
		/// Get the node that was parsed. Cached for convenience.
		/// </summary>
		/// <returns>Node</returns>
		public XmlNode GetNode()
		{
			return node;
		}

		/// <summary>
		/// Get the xml name of the main node.
		/// </summary>
		/// <returns>Data type</returns>
		public string GetDataType()
		{
			return node.Name;
		}

		protected virtual bool HasNameOverride(string name)
		{
			return false;
		}

		public List<XmlData> SearchAllByName(string name, bool recursive = false)
		{
			List<XmlData> results = new List<XmlData>();
			SearchAllByName(results, name, recursive);
			return results;
		}

		public void SearchAllByName(List<XmlData> results, string name, bool recursive = false)
		{
			INameable nameable = this as INameable;
			if (nameable != null && nameable.GetName() == name)
				results.Add(this);
			else if (HasNameOverride(name))
				results.Add(this);

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					INameable field = data as INameable;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetName() == name)
					{
						results.Add(data); // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						data.SearchAllByName(results, name, recursive);
					}
				}
			}
		}

		public List<XmlData> SearchAllById(string id, bool recursive = false)
		{
			List<XmlData> results = new List<XmlData>();
			SearchAllById(results, id, recursive);
			return results;
		}

		public void SearchAllById(List<XmlData> results, string id, bool recursive = false)
		{
			IIdentifiable identifiable = this as IIdentifiable;
			if (identifiable != null && identifiable.GetId() == id)
				results.Add(this);

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					IIdentifiable field = data as IIdentifiable;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetId() == id)
					{
						results.Add(data); // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						data.SearchAllById(results, id, recursive);
					}
				}
			}
		}

		public List<XmlData> SearchAllById(string id, string type, bool recursive = false)
		{
			List<XmlData> results = new List<XmlData>();
			SearchAllById(results, id, type, recursive);
			return results;
		}

		public void SearchAllById(List<XmlData> results, string id, string type, bool recursive = false)
		{
			IIdentifiable identifiable = this as IIdentifiable;
			if (identifiable != null && identifiable.GetId() == id)
				results.Add(this);

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					if (data.GetDataType() != type)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					IIdentifiable field = data as IIdentifiable;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetId() == id)
					{
						results.Add(data); // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						data.SearchAllById(results, id, recursive);
					}
				}
			}
		}

		public void SearchAllByName<T>(List<T> results, string name, bool recursive = false) where T : XmlData, INameable
		{
			T nameable = this as T;
			if (nameable != null)
			{
				if (nameable.GetName() == name || HasNameOverride(name))
					results.Add(nameable);
			}

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					T field = data as T;
					if (field == null)
					{
						if (recursive) // make sure to still search for these within
						{
							data.SearchAllByName<T>(results, name, recursive);
							continue;
						}
						else
						{
							break; // we don't mix and match types in the lists normally, so we can exit the loop
						}
					}

					if (field.GetName() == name)
					{
						results.Add(field); // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						data.SearchAllByName<T>(results, name, recursive);
					}
				}
			}
		}

		public void SearchAllById<T>(List<T> results, string id, bool recursive = false) where T : XmlData, IIdentifiable
		{
			T identifiable = this as T;
			if (identifiable != null && identifiable.GetId() == id)
				results.Add(identifiable);

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					T field = data as T;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetId() == id)
					{
						results.Add(field); // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						data.SearchAllById<T>(results, id, recursive);
					}
				}
			}
		}

		public List<T> SearchAllByName<T>(string name, bool recursive = false) where T : XmlData, INameable
		{
			List<T> results = new List<T>();
			SearchAllByName<T>(results, name, recursive);
			return results;
		}

		public List<T> SearchAllById<T>(string id, bool recursive = false) where T : XmlData, IIdentifiable
		{
			List<T> results = new List<T>();
			SearchAllById<T>(results, id, recursive);
			return results;
		}

		public T SearchByName<T>(string name, bool recursive = false) where T : XmlData, INameable
		{
			T nameable = this as T;
			if (nameable != null)
			{
				if (nameable.GetName() == name || HasNameOverride(name))
					return nameable;
			}

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					T field = data as T;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetName() == name)
					{
						return field; // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						T result = data.SearchByName<T>(name, recursive);
						if (result != null)
							return result;
					}
				}
			}

			return null;
		}

		public T SearchById<T>(string id, bool recursive = false) where T : XmlData, IIdentifiable
		{
			T identifiable = this as T;
			if (identifiable != null && identifiable.GetId() == id)
				return identifiable;

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					T field = data as T;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetId() == id)
					{
						return field; // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						T result = data.SearchById<T>(id, recursive);
						if (result != null)
							return result;
					}
				}
			}

			return null;
		}

		public XmlData SearchByName(string name, bool recursive = false)
		{
			INameable nameable = this as INameable;
			if (nameable != null && nameable.GetName() == name)
				return this;
			else if (HasNameOverride(name))
				return this;

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					INameable field = data as INameable;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetName() == name)
					{
						return data; // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						XmlData result = data.SearchByName(name, recursive);
						if (result != null)
							return result;
					}
				}
			}

			return null;
		}

		public XmlData SearchById(string id, bool recursive = false)
		{
			IIdentifiable identifiable = this as IIdentifiable;
			if (identifiable != null && identifiable.GetId() == id)
				return this;

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					IIdentifiable field = data as IIdentifiable;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					if (field.GetId() == id)
					{
						return data; // this does not search recursively it assumes we have found it and need to look no further, change maybe?
					}
					else if (recursive)
					{
						XmlData result = data.SearchById(id, recursive);
						if (result != null)
							return result;
					}
				}
			}

			return null;
		}

		//public XmlData SearchById(string id, string type, bool recursive = false)
		//{
		//	IIdentifiable identifiable = this as IIdentifiable;
		//	if (identifiable != null && identifiable.GetId() == id)
		//		return this;

		//	foreach (IList list in fields)
		//	{
		//		foreach (XmlData data in list)
		//		{
		//			if (data.GetDataType() != type)
		//				break; // we don't mix and match types in the lists normally, so we can exit the loop

		//			IIdentifiable field = data as IIdentifiable;
		//			if (field == null)
		//				break; // we don't mix and match types in the lists normally, so we can exit the loop

		//			if (field.GetId() == id)
		//			{
		//				return data; // this does not search recursively it assumes we have found it and need to look no further, change maybe?
		//			}
		//			else if (recursive)
		//			{
		//				XmlData result = data.SearchById(id, recursive);
		//				if (result != null)
		//					return result;
		//			}
		//		}
		//	}

		//	return null;
		//}

		public List<T> GetAll<T>(bool recursive = false) where T : XmlData
		{
			List<T> results = new List<T>();

			foreach (IList list in fields)
			{
				foreach (XmlData data in list)
				{
					T field = data as T;
					if (field == null)
						break; // we don't mix and match types in the lists normally, so we can exit the loop

					results.Add(field);

					// recursive
					if (recursive)
					{
						results.AddRange(field.GetAll<T>());
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Helper to write a list of XmlData using <see cref="WriteXml(XmlWriter)"/> on each member in the list. If the list is null or empty it does not write anything.
		/// </summary>
		/// <typeparam name="T">Type of XmlData</typeparam>
		/// <param name="writer">Xml Writer</param>
		/// <param name="list">List to write</param>
		/// <param name="elementName">Element name to contain these</param>
		protected void WriteXmlList<T>(XmlWriter writer, List<T> list, string elementName) where T : XmlData
		{
			// don't write empty lists, not the xml way
			if (list == null || list.Count == 0)
				return;

			writer.WriteStartElement(elementName);
			foreach (T entry in list)
			{
				entry.WriteXml(writer);
			}
			writer.WriteEndElement();
		}
	}
}
