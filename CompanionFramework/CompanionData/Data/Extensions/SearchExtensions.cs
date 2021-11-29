using System.Collections.Generic;

namespace Companion.Data
{
	public static class SearchExtensions
	{
		//public static T GetByName<T>(this List<XmlData> list, string name) where T : XmlData, INameable
		//{
		//	foreach (XmlData data in list)
		//	{
		//		if (data is INameable)
		//		{
		//			T type = (T)data;
		//			if (type.GetName() == name)
		//				return type;
		//		}
		//	}

		//	return null;
		//}

		public static bool ContainsName<T>(this List<T> list, string name) where T : XmlData, INameable
		{
			foreach (T data in list)
			{
				if (data.GetName() != null && data.GetName().Contains(name))
					return true;
			}

			return false;
		}

		public static T GetContainingName<T>(this List<T> list, string name) where T : XmlData, INameable
		{
			foreach (T data in list)
			{
				if (data.GetName() != null && data.GetName().Contains(name))
					return data;
			}

			return null;
		}

		public static List<XmlData> GetTargets(this List<EntryLink> entryLinks, GameSystem gameSystem)
		{
			List<XmlData> results = new List<XmlData>();
			foreach (EntryLink entryLink in entryLinks)
			{
				XmlData target = entryLink.GetTarget(gameSystem);
				if (target != null)
					results.Add(target);
			}

			return results;
		}

		public static List<T> GetTargets<T>(this List<EntryLink> entryLinks, GameSystem gameSystem) where T : XmlData
		{
			List<T> results = new List<T>();
			foreach (EntryLink entryLink in entryLinks)
			{
				XmlData target = entryLink.GetTarget(gameSystem);
				if (target != null)
				{
					if (target is T)
						results.Add((T)target);
					else
						results.AddRange(target.GetAll<T>(true));
				}
			}

			return results;
		}

		public static List<T> GetFromList<T>(this List<XmlData> list) where T : XmlData
		{
			List<T> filteredList = new List<T>();

			foreach (XmlData entry in list)
			{
				T test = entry as T;

				if (test != null)
				{
					filteredList.Add(test);
				}
			}

			return filteredList;
		}

		public static T GetByName<T>(this List<T> list, string name) where T : INameable
		{
			if (list == null)
				return default(T);

			foreach (T t in list)
			{
				if (t != null && t.GetName() == name)
				{
					return t;
				}
				else if (t is ISearchableByName)
				{
					ISearchableByName searchable = (ISearchableByName)t;
					XmlData result = searchable.SearchByName(name);
					if (result != null)
						return t;
				}
			}

			return default(T);
		}

		public static List<T> GetAllByName<T>(this List<T> list, string name) where T : INameable
		{
			List<T> results = new List<T>();

			foreach (T t in list)
			{
				if (t != null && t.GetName() == name)
				{
					results.Add(t);
				}
				else if (t is ISearchableByName)
				{
					ISearchableByName searchable = (ISearchableByName)t;
					XmlData result = searchable.SearchByName(name);
					if (result != null)
					{
						results.Add(t);
					}
				}
			}

			return results;
		}

		public static T GetById<T>(this List<T> list, string id) where T : IIdentifiable
		{
			if (list == null)
				return default(T);

			foreach (T t in list)
			{
				if (t != null && t.GetId() == id)
				{
					return t;
				}
				else if (t is ISearchableById)
				{
					ISearchableById searchable = (ISearchableById)t;
					XmlData result = searchable.SearchById(id);
					if (result != null)
						return t;
				}
			}

			return default(T);
		}

		public static List<T> GetAllById<T>(this List<T> list, string id) where T : IIdentifiable
		{
			List<T> results = new List<T>();

			foreach (T t in list)
			{
				if (t != null && t.GetId() == id)
				{
					results.Add(t);
				}
				else if (t is ISearchableById)
				{
					ISearchableById searchable = (ISearchableById)t;
					XmlData result = searchable.SearchById(id);
					if (result != null)
						results.Add(t);
				}
			}

			return results;
		}
	}
}
