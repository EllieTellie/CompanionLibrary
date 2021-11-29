using Companion.Data;
using CompanionFramework.Core.Threading.Messaging;
using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class SystemManager
{
	private Dictionary<GameSystem, List<Catalogue>> gameSystems = new Dictionary<GameSystem, List<Catalogue>>();

	private static SystemManager instance;

	/// <summary>
	/// Dispatched on the main thread. Source is this. EventArgs is null.
	/// </summary>
	public event EventHandler OnGameSystemsLoaded;

	private bool loading = false;

	public static SystemManager Instance
	{
		get
		{
			if (instance == null)
				instance = new SystemManager();

			return instance;
		}
	}

	public void LoadGameSystemsAsync(string path)
	{
		if (loading)
			return;

		loading = true;
		ThreadPool.QueueUserWorkItem(LoadGameSystemsAsync, path);
	}

	private void LoadGameSystemsAsync(object state)
	{
		string path = (string)state;
		LoadGameSystems(path);

		loading = false; // bit worrying that load game systems could throw exception and leave this as true, handle that

		// fire this to the main thread
		MessageQueue.Invoke(OnGameSystemsLoaded, this);
	}

	public void LoadGameSystems(string path)
	{
		if (!Directory.Exists(path))
			return;

		List<string> gameSystemFiles = FileSearchUtils.FindFileNamesByExtension(path, ".gstz", 2);

		foreach (string gameSystem in gameSystemFiles)
		{
			GameSystem game = GameSystem.LoadGameSystem(gameSystem);

			if (game != null)
			{
				List<Catalogue> catalogues = new List<Catalogue>();
				gameSystems.Add(game, catalogues);

				string directory = FileUtils.GetDirectoryFromPath(gameSystem);

				// load catalogues
				List<string> catalogueFiles = FileSearchUtils.FindFileNamesByExtension(directory, ".catz", 1);

				foreach (string file in catalogueFiles)
				{
					Catalogue catalogue = Catalogue.LoadCatalogue(file);

					if (catalogue != null)
					{
						catalogues.Add(catalogue);
					}
				}
			}
		}
	}

	public bool HasGameSystems()
	{
		return gameSystems.Count > 0;
	}

	public Catalogue GetCatalogueById(GameSystem gameSystem, string id)
	{
		List<Catalogue> catalogues = GetCatalogues(gameSystem);

		foreach (Catalogue catalogue in catalogues)
		{
			if (catalogue.id == id)
			{
				return catalogue;
			}
		}

		return null;
	}

	public GameSystem GetGameSystemById(string id)
	{
		foreach (GameSystem gameSystem in gameSystems.Keys)
		{
			if (gameSystem.id == id)
				return gameSystem;
		}

		return null;
	}

	public GameSystem GetGameSystemByName(string name)
	{
		foreach (GameSystem gameSystem in gameSystems.Keys)
		{
			if (gameSystem.name == name)
				return gameSystem;
		}

		return null;
	}

	public List<GameSystem> GetGameSystems()
	{
		List<GameSystem> systems = new List<GameSystem>();
		foreach (GameSystem gameSystem in gameSystems.Keys)
		{
			systems.Add(gameSystem);
		}

		return systems;
	}

	public List<Catalogue> GetCatalogues(GameSystem gameSystem)
	{
		List<Catalogue> catalogues;

		if (!gameSystems.TryGetValue(gameSystem, out catalogues))
			catalogues = new List<Catalogue>();

		return catalogues;
	}

	public T SearchByName<T>(GameSystem gameSystem, string name, bool recursive = false) where T : XmlData, INameable
	{
		List<T> results = SearchAllByName<T>(gameSystem, name, recursive);

		// this can return slightly different results because the order is different in the lists
		//List<XmlData> resultsOld = SearchAllByName(gameSystem, name, recursive);
		//T resultOld = null;
		//foreach (XmlData result in resultsOld)
		//{
		//	T test = result as T;
		//	if (test != null)
		//		resultOld = test;
		//}

		return results != null && results.Count > 0 ? results[0] : null;
	}

	public T SearchById<T>(GameSystem gameSystem, string id, bool recursive = false) where T : XmlData, IIdentifiable
	{
		List<T> results = SearchAllById<T>(gameSystem, id, recursive);
		return results != null && results.Count > 0 ? results[0] : null;
	}

	public XmlData SearchByName(GameSystem gameSystem, string name, bool recursive = false)
	{
		XmlData result = gameSystem.SearchByName(name, recursive);
		if (result != null)
			return result;

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			result = cat.SearchByName(name, recursive);
			if (result != null)
				return result;
		}

		return null;
	}

	public XmlData SearchById(GameSystem gameSystem, string id, bool recursive = false)
	{
		XmlData result = gameSystem.SearchById(id, recursive);
		if (result != null)
			return result;

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			result = cat.SearchById(id, recursive);
			if (result != null)
				return result;
		}

		return null;
	}

	//public XmlData SearchById(GameSystem gameSystem, string id, string type, bool recursive = false)
	//{
	//	XmlData result = gameSystem.SearchById(id, type, recursive);
	//	if (result != null)
	//		return result;

	//	List<Catalogue> catalogues = gameSystems[gameSystem];
	//	foreach (Catalogue cat in catalogues)
	//	{
	//		result = cat.SearchById(id, type, recursive);
	//		if (result != null)
	//			return result;
	//	}

	//	return null;
	//}

	public List<XmlData> SearchAllByName(GameSystem gameSystem, string name, bool recursive = false)
	{
		List<XmlData> results = new List<XmlData>();

		gameSystem.SearchAllByName(results, name, recursive);

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			cat.SearchAllByName(results, name, recursive);
		}

		return results;
	}

	public List<XmlData> SearchAllById(GameSystem gameSystem, string id, bool recursive = false)
	{
		List<XmlData> results = new List<XmlData>();

		gameSystem.SearchAllById(results, id, recursive);

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			cat.SearchAllById(results, id, recursive);
		}

		return results;
	}

	public List<T> SearchAllByName<T>(GameSystem gameSystem, string name, bool recursive = false) where T : XmlData, INameable
	{
		List<T> results = new List<T>();

		gameSystem.SearchAllByName<T>(results, name, recursive);

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			cat.SearchAllByName<T>(results, name, recursive);
		}

		return results;
	}

	public List<T> SearchAllById<T>(GameSystem gameSystem, string id, bool recursive = false) where T : XmlData, IIdentifiable
	{
		List<T> results = new List<T>();

		gameSystem.SearchAllById<T>(results, id, recursive);

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			cat.SearchAllById<T>(results, id, recursive);
		}

		return results;
	}

	public List<XmlData> SearchAllById(GameSystem gameSystem, string id, string type, bool recursive = false)
	{
		List<XmlData> results = new List<XmlData>();

		gameSystem.SearchAllById(results, id, type, recursive);

		List<Catalogue> catalogues = gameSystems[gameSystem];
		foreach (Catalogue cat in catalogues)
		{
			cat.SearchAllById(results, id, type, recursive);
		}

		return results;
	}

	public List<EntryLink> GetEntryLinks(GameSystem gameSystem, string id)
	{
		List<EntryLink> entryLinks = new List<EntryLink>();
		GetEntryLinks(entryLinks, gameSystem, id);
		return entryLinks;
	}

	private void GetEntryLinks(List<EntryLink> entryLinks, GameSystem gameSystem, string targetId)
	{
		List<XmlData> results = SearchAllById(gameSystem, targetId, true); // always recursive

		foreach (XmlData result in results)
		{
			if (result is EntryLink)
				entryLinks.Add((EntryLink)result);

			List<EntryLink> subLinks = result.GetAll<EntryLink>(true);
			foreach (EntryLink subLink in subLinks)
			{
				if (!ContainsTargetId(entryLinks, subLink.id))
				{
					entryLinks.Add(subLink);

					if (!ContainsTargetId(entryLinks, subLink.targetId))
						GetEntryLinks(gameSystem, subLink.targetId);
				}
			}
		}
	}

	private bool ContainsTargetId(List<EntryLink> entryLinks, string id)
	{
		return entryLinks.GetById(id) != null;
	}
}
