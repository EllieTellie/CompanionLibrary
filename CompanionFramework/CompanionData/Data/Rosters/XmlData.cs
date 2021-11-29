﻿using Companion.Data.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public abstract class XmlData
	{
		/// <summary>
		/// Optional comment for data.
		/// </summary>
		public string comment;

		protected readonly XmlNode node;

		/// <summary>
		/// List of xml lists containing all the nodes after parsing.
		/// </summary>
		protected List<IList> fields = new List<IList>();

		public XmlData(XmlNode node)
		{
			this.node = node;

			if (node != null)
			{
				OnParseNode();

				OnParseGlobalData();
			}
			else
			{
				InitFields();
			}
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

		protected virtual void OnParseGlobalData()
		{
			XmlNode commentNode = node.GetNode("comment");
			if (commentNode != null)
				comment = commentNode.InnerText;
		}

		protected abstract void OnParseNode();

		public T ParseXml<T>(XmlNode node) where T : XmlData
		{
			return XmlUtils.ParseXml<T>(node);
		}

		public List<T> ParseXmlList<T>(List<XmlNode> nodes) where T : XmlData
		{
			List<T> results = XmlUtils.ParseXmlList<T>(nodes);
			AddField(results); // automatically add
			return results;
		}

		public XmlNode GetNode()
		{
			return node;
		}

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
	}
}
