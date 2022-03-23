using Companion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RosterManager
{
	private List<Roster> rosters = new List<Roster>();

	private static RosterManager instance;

	/// <summary>
	/// Fired when all rosters are loaded.
	/// </summary>
	public event Action OnRostersLoaded;

	/// <summary>
	/// Fired whenever a roster is added.
	/// </summary>
	public event Action<Roster> OnRostersAdded;

	/// <summary>
	/// Fired whenever a roster is added.
	/// </summary>
	public event Action<Roster> OnRostersRemoved;

	public static RosterManager Instance
	{
		get
		{
			if (instance == null)
				instance = new RosterManager();

			return instance;
		}
	}

	public void SetRosters(List<Roster> rosters)
	{
		this.rosters = rosters;
	}

	public List<Roster> GetRosters()
	{
		return rosters;
	}

	public Roster GetRosterByName(string name)
	{
		foreach (Roster roster in rosters)
		{
			if (roster.name == name)
				return roster;
		}

		return null;
	}

	public Roster GetRosterById(string id)
	{
		foreach (Roster roster in rosters)
		{
			if (roster.id == id)
				return roster;
		}

		return null;
	}

	public void AddRoster(Roster roster)
	{
		rosters.Add(roster);

		if (OnRostersAdded != null)
			OnRostersAdded(roster);
	}

	public bool HasRostersLoaded()
	{
		return rosters.Count > 0;
	}

	public void SetLoaded()
	{
		if (OnRostersLoaded != null)
			OnRostersLoaded();
	}

	/// <summary>
	/// Parse the roster and add it to the roster manager.
	/// </summary>
	/// <param name="data">Roster data</param>
	public Roster AddRoster(byte[] data)
	{
		Roster roster = Roster.LoadRosterXml(data);
		if (roster != null)
			AddRoster(roster);

		return roster;
	}

	/// <summary>
	/// Remove the roster if present.
	/// </summary>
	/// <param name="roster">Roster</param>
	/// <returns>True if removed</returns>
	public bool RemoveRoster(Roster roster)
    {
		if (rosters.Remove(roster))
        {
			if (OnRostersRemoved != null)
				OnRostersRemoved(roster);

			return true;
        }

		return false;
    }

	//public void ReadRoster(string path, Action<Roster> onRosterLoaded = null)
	//{
	//	Coroutiner.Start(ReadRosterInternal(path, onRosterLoaded));
	//}

	//public void ReadRostersFromStreamingAssets(Action onRostersLoaded = null)
	//{
	//	string path = Application.streamingAssetsPath;

	//	Coroutiner.Start(ReadStreamingRosters(onRostersLoaded));
	//}

	//private IEnumerator ReadStreamingRosters(Action onRostersLoaded = null)
	//{
	//	string path = Application.streamingAssetsPath;

	//	// read index file
	//	using (UnityWebRequest request = UnityWebRequest.Get(Path.Combine(path, "index.bytes")))
	//	{
	//		request.SendWebRequest();

	//		// just wait until done
	//		while (!request.isDone)
	//		{
	//			yield return null;
	//		}

	//		string text = request.downloadHandler.text;

	//		StringReader reader = new StringReader(text);
	//		string line;

	//		while ((line = reader.ReadLine()) != null)
	//		{
	//			string fileName = line;

	//			// skip anything that's not a roster
	//			if (!fileName.EndsWith(".rosz"))
	//				continue;

	//			yield return ReadRosterInternal(Path.Combine(path, fileName));
	//		}
	//	}

	//	if (onRostersLoaded != null)
	//		onRostersLoaded();

	//	if (OnRostersLoaded != null)
	//		OnRostersLoaded();
	//}

	//protected IEnumerator ReadRosterInternal(string path, Action<Roster> onRosterLoaded = null)
	//{
	//	using (UnityWebRequest request = UnityWebRequest.Get(path))
	//	{
	//		request.SendWebRequest();

	//		// just wait until done
	//		while (!request.isDone)
	//		{
	//			yield return null;
	//		}

	//		byte[] data = request.downloadHandler.data;
	//		Roster roster = Roster.LoadRosterXml(data);

	//		if (roster != null)
	//			AddRoster(roster);

	//		if (onRosterLoaded != null)
	//			onRosterLoaded(roster);
	//	}
	//}
}
