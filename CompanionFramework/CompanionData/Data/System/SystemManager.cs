using Companion.Data;
using CompanionFramework.Core.Threading.Messaging;
using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/// <summary>
/// System Manager is used to store game systems and catalogues.
/// </summary>
public class SystemManager
{
	private List<GameSystemGroup> gameSystemGroups = new List<GameSystemGroup>();

	private static SystemManager instance;

	/// <summary>
	/// Dispatched on the main thread. Source is this. EventArgs is null.
	/// </summary>
	public event EventHandler OnGameSystemsLoaded;

	private bool loading = false;

	/// <summary>
	/// Get the global system manager. If no system manager is available this call creates a new one.
	/// </summary>
	public static SystemManager Instance
	{
		get
		{
			if (instance == null)
				instance = new SystemManager();

			return instance;
		}
	}

	/// <summary>
	/// Load all game systems at this path using the ThreadPool. Use <see cref="OnGameSystemsLoaded"/> for the callback. The callback is on the main thread if the <see cref="MessageQueue"/> is used otherwise it is not.
	/// </summary>
	/// <param name="path">Path to load game systems from</param>
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
		if (MessageHandler.HasMessageHandler())
		{
			MessageQueue.Invoke(OnGameSystemsLoaded, this); // called delayed on main thread
		}
		else
		{
			if (OnGameSystemsLoaded != null) // called on thread pool thread
				OnGameSystemsLoaded(this, null);
		}
	}

	/// <summary>
	/// Detect any game systems in the immediate path and any subfolders. This uses I/O to find files.
	/// </summary>
	/// <param name="path">Path</param>
	/// <returns>List of game system file paths</returns>
	public List<string> DetectGameSystems(string path)
	{
		if (!Directory.Exists(path))
			return new List<string>();

		return FileSearchUtils.FindFileNamesByExtension(path, ".gstz", 2);
	}

	/// <summary>
	/// Load all game systems at the specified path. This loads any required catalogues too.
	/// </summary>
	/// <param name="path">Path</param>
	public void LoadGameSystems(string path)
	{
		List<string> gameSystemFiles = DetectGameSystems(path);

		foreach (string gameSystem in gameSystemFiles)
		{
			GameSystem game = GameSystem.LoadGameSystem(gameSystem);

			if (game != null)
			{
				List<Catalogue> catalogues = new List<Catalogue>();
				gameSystemGroups.Add(new GameSystemGroup(game, catalogues));

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

	/// <summary>
	/// Returns true if any game systems are loaded.
	/// </summary>
	/// <returns>True if game systems are loaded</returns>
	public bool HasGameSystems()
	{
		return gameSystemGroups.Count > 0;
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

	/// <summary>
	/// Get game system group by the game system id. Can return null if not found.
	/// </summary>
	/// <param name="id">Game system id</param>
	/// <returns>Game system group or null</returns>
	public GameSystemGroup GetGameSystemGroupById(string id)
	{
		foreach (GameSystemGroup gameSystemGroup in gameSystemGroups)
		{
			if (gameSystemGroup.gameSystem.id == id)
				return gameSystemGroup;
		}

		return null;
	}

	/// <summary>
	/// Get game system group by the game system name. Can return null if not found.
	/// </summary>
	/// <param name="name">Game system name</param>
	/// <returns>Game system group or null</returns>
	public GameSystemGroup GetGameSystemGroupByName(string name)
	{
		foreach (GameSystemGroup gameSystemGroup in gameSystemGroups)
		{
			if (gameSystemGroup.gameSystem.name == name)
				return gameSystemGroup;
		}

		return null;
	}

	/// <summary>
	/// Get game system by the game system id. Can return null if not found.
	/// </summary>
	/// <param name="id">Game system id</param>
	/// <returns>Game system or null</returns>
	public GameSystem GetGameSystemById(string id)
	{
		foreach (GameSystemGroup gameSystemGroup in gameSystemGroups)
		{
			if (gameSystemGroup.gameSystem.id == id)
				return gameSystemGroup.gameSystem;
		}

		return null;
	}

	/// <summary>
	/// Get game system by the game system name. Can return null if not found.
	/// </summary>
	/// <param name="name">Game system name</param>
	/// <returns>Game system or null</returns>
	public GameSystem GetGameSystemByName(string name)
	{
		foreach (GameSystemGroup gameSystemGroup in gameSystemGroups)
		{
			if (gameSystemGroup.gameSystem.name == name)
				return gameSystemGroup.gameSystem;
		}

		return null;
	}

	/// <summary>
	/// Get all the game system groups. This list is passed by reference.
	/// </summary>
	/// <returns>Game system groups</returns>
	public List<GameSystemGroup> GetGameSystemGroups()
	{
		return gameSystemGroups;
	}

	/// <summary>
	/// Get all the game systems. This creates a new list every time.
	/// </summary>
	/// <returns>List of game systems</returns>
	public List<GameSystem> GetGameSystems()
	{
		List<GameSystem> systems = new List<GameSystem>();
		foreach (GameSystemGroup gameSystemGroup in gameSystemGroups)
		{
			systems.Add(gameSystemGroup.gameSystem);
		}

		return systems;
	}

	protected GameSystemGroup GetGameSystemGroup(GameSystem gameSystem)
	{
		foreach (GameSystemGroup gameSystemGroup in gameSystemGroups)
		{
			if (gameSystemGroup.gameSystem == gameSystem)
				return gameSystemGroup;
		}

		return null;
	}

	/// <summary>
	/// Get a list of catalogues that are loaded for this game system. Returns an empty list if no catalogues are loaded.
	/// </summary>
	/// <param name="gameSystem">Game system</param>
	/// <returns>List of catalogues</returns>
	public List<Catalogue> GetCatalogues(GameSystem gameSystem)
	{
		GameSystemGroup gameSystemGroup = GetGameSystemGroup(gameSystem);
		if (gameSystemGroup == null)
			return new List<Catalogue>();
		else
			return gameSystemGroup.GetCatalogues();
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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

		List<Catalogue> catalogues = GetCatalogues(gameSystem);
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
