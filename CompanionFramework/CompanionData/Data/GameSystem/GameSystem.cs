using Companion.Data.System.Update;
using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class GameSystem : XmlData, IIdentifiable, INameable, IRootContainer
	{
		public string id;
		public string name;
		public string revision;
		public string battleScribeVersion;
		public string authorName;
		public string authorContact;
		public string authorUrl;

		public string readme;

		public List<Publication> publications;
		public List<CostType> costTypes;
		public List<ProfileType> profileTypes;
		public List<CategoryEntry> categoryEntries;
		public List<ForceEntry> forceEntries;
		public List<EntryLink> entryLinks;
		public List<SelectionEntry> sharedSelectionEntries;
		public List<SelectionEntryGroup> sharedSelectionEntryGroups;
		public List<Rule> sharedRules;
		public List<Profile> sharedProfiles;

		/// <summary>
		/// Path this was read from (if available).
		/// </summary>
		private string path;

		private Dictionary<string, IIdentifiable> idLookup = new Dictionary<string, IIdentifiable>();

		public GameSystem(XmlNode node) : base(node)
		{
		}

		public IIdentifiable GetIdentifiable(string id)
		{
			if (idLookup.TryGetValue(id, out IIdentifiable identifiable))
				return identifiable;
			else
				return null;
		}

		public bool HasId(string uniqueId)
		{
			return idLookup.ContainsKey(uniqueId);
		}

		public string GenerateUniqueId()
        {
			string guid = Guid.NewGuid().ToString();
			int count = 0;
			
			// check for duplicates
			while (HasId(guid))
            {
				count++;
				if (count >= 1000) // something is really wrong here
                {
					FrameworkLogger.Error("Exhausted 1000 tries to generate a non matching guid");
					return guid; // just return this one in that case
                }

				// attempt to generate a new one
				guid = Guid.NewGuid().ToString();
			}

			return guid;
		}

		public void AddIdLookup(IIdentifiable identifiable)
		{
			string id = identifiable.GetId();

			if (idLookup.ContainsKey(id))
			{
				FrameworkLogger.Error("Id " + id + " already present");
			}
			else
			{
				idLookup[id] = identifiable;
			}
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			revision = node.GetAttribute("revision");
			battleScribeVersion = node.GetAttribute("battleScribeVersion");
			authorName = node.GetAttribute("authorName");
			authorContact = node.GetAttribute("authorContact");
			authorUrl = node.GetAttribute("authorUrl");

			XmlNode readmeNode = node.GetNode("readme");
			readme = readmeNode != null ? readmeNode.InnerText : null;

			publications = ParseXmlList<Publication>(node.GetNodesFromPath("publications", "publication"), rootContainer);
			costTypes = ParseXmlList<CostType>(node.GetNodesFromPath("costTypes", "costType"), rootContainer);
			profileTypes = ParseXmlList<ProfileType>(node.GetNodesFromPath("profileTypes", "profileType"), rootContainer);
			categoryEntries = ParseXmlList<CategoryEntry>(node.GetNodesFromPath("categoryEntries", "categoryEntry"), rootContainer);
			forceEntries = ParseXmlList<ForceEntry>(node.GetNodesFromPath("forceEntries", "forceEntry"), rootContainer);
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"), rootContainer);
			sharedSelectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("sharedSelectionEntries", "selectionEntry"), rootContainer);
			sharedSelectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("sharedSelectionEntryGroups", "selectionEntryGroup"), rootContainer);
			sharedRules = ParseXmlList<Rule>(node.GetNodesFromPath("sharedRules", "rule"), rootContainer);
			sharedProfiles = ParseXmlList<Profile>(node.GetNodesFromPath("sharedProfiles", "profile"), rootContainer);
		}

		public static GameSystem LoadGameSystem(string path)
		{
			try
			{
				//byte[] data = FileUtils.ReadFileSimple(path);

				// unzip data
				//byte[] uncompressedData = Decompress(data);

				//string text = DecompressText(data); //FileUtils.GetString(uncompressedData);
				//xmlDocument.LoadXml(text);

				XmlDocument xmlDocument = DecompressXml(path);

				GameSystem gameSystem = new GameSystem(xmlDocument.GetNode("gameSystem"));
				gameSystem.path = path;
				return gameSystem;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}

		/// <summary>
		/// Get version information from the game system at the specified path.
		/// </summary>
		/// <param name="path">Path</param>
		/// <returns>Version information</returns>
		public static DataIndexVersionInfo GetVersionInfo(string path)
        {
			byte[] data = FileUtils.ReadFile(path);
			return DataIndexVersionInfo.GetVersionInfo(data, ".gst", "gameSystem");
		}
		

		public static byte[] Decompress(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".gst");
		}

		public static XmlDocument DecompressXml(byte[] data)
		{
			return CompressionUtils.DecompressXmlDocumentFromZip(data, ".gst");
		}

		public static XmlDocument DecompressXml(string path)
		{
			return CompressionUtils.DecompressXmlDocumentFromZipFile(path, ".gst");
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		public string GetPath()
		{
			return path;
		}
	}
}