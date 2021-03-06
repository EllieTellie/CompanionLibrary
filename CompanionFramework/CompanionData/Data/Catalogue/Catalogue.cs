using Companion.Data.System.Update;
using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Catalogue : XmlData, INameable, IIdentifiable, IRootContainer
	{
		public string id;
		public string name;
		public string revision;
		public string battleScribeVersion;
		public string authorName;
		public string authorContact;
		public string authorUrl;
		public bool library;
		public string gameSystemId;
		public string gameSystemRevision;

		public string readme;

		public List<Publication> publications;
		public List<ProfileType> profileTypes;
		public List<CategoryEntry> categoryEntries;
		public List<SelectionEntry> selectionEntries;
		public List<EntryLink> entryLinks;
		public List<SelectionEntry> sharedSelectionEntries;
		public List<SelectionEntryGroup> sharedSelectionEntryGroups;
		public List<Rule> sharedRules;
		public List<Rule> rules;
		public List<Profile> sharedProfiles;
		public List<InfoGroup> sharedInfoGroups;
		public List<CatalogueLink> catalogueLinks;
		public List<InfoLink> infoLinks;
		public List<CostType> costTypes;
		public List<ForceEntry> forceEntries;

		/// <summary>
		/// Path this was read from (if available).
		/// </summary>
		private string path;

		private Dictionary<string, IIdentifiable> idLookup = new Dictionary<string, IIdentifiable>();

		public Catalogue(XmlNode node) : base(node)
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

		public void AddIdLookup(IIdentifiable identifiable)
		{
			string id = identifiable.GetId();

			if (idLookup.ContainsKey(id))
			{
				IIdentifiable other = idLookup[id];

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
			library = node.GetAttributeBool("library");
			gameSystemId = node.GetAttribute("gameSystemId");
			gameSystemRevision = node.GetAttribute("gameSystemRevision");

			XmlNode readmeNode = node.GetNode("readme");
			readme = readmeNode != null ? readmeNode.InnerText : null;

			publications = ParseXmlList<Publication>(node.GetNodesFromPath("publications", "publication"), rootContainer);
			profileTypes = ParseXmlList<ProfileType>(node.GetNodesFromPath("profileTypes", "profileType"), rootContainer);
			categoryEntries = ParseXmlList<CategoryEntry>(node.GetNodesFromPath("categoryEntries", "categoryEntry"), rootContainer);
			selectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("selectionEntries", "selectionEntry"), rootContainer);
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"), rootContainer);
			sharedSelectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("sharedSelectionEntries", "selectionEntry"), rootContainer);
			sharedSelectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("sharedSelectionEntryGroups", "selectionEntryGroup"), rootContainer);
			sharedRules = ParseXmlList<Rule>(node.GetNodesFromPath("sharedRules", "rule"), rootContainer);
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"), rootContainer);
			sharedProfiles = ParseXmlList<Profile>(node.GetNodesFromPath("sharedProfiles", "profile"), rootContainer);
			catalogueLinks = ParseXmlList<CatalogueLink>(node.GetNodesFromPath("catalogueLinks", "catalogueLink"), rootContainer);
			sharedInfoGroups = ParseXmlList<InfoGroup>(node.GetNodesFromPath("sharedInfoGroups", "infoGroup"), rootContainer);
			infoLinks = ParseXmlList<InfoLink>(node.GetNodesFromPath("infoLinks", "infoLink"), rootContainer);
			costTypes = ParseXmlList<CostType>(node.GetNodesFromPath("costTypes", "costType"), rootContainer);
			forceEntries = ParseXmlList<ForceEntry>(node.GetNodesFromPath("forceEntries", "forceEntry"), rootContainer);
		}

		public static Catalogue LoadCatalogue(string path)
		{
			try
			{
				XmlDocument xmlDocument = null;
				if (path.EndsWith(".cat"))
				{
					string text = FileUtils.ReadTextFile(path);
					if (text != null)
					{
						xmlDocument = new XmlDocument();
						xmlDocument.LoadXml(text);
					}
				}
				else
				{
					xmlDocument = DecompressXml(path);
				}

				if (xmlDocument == null)
					return null;

				Catalogue catalogue = new Catalogue(xmlDocument.GetNode("catalogue"));
				catalogue.path = path; // store path in case we need it

				return catalogue;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}

		}

		/// <summary>
		/// Get version information from the catalogue at the specified path.
		/// </summary>
		/// <param name="path">Path</param>
		/// <returns>Version information</returns>
		public static DataIndexVersionInfo GetVersionInfo(string path)
		{
			byte[] data = FileUtils.ReadFile(path);
			return DataIndexVersionInfo.GetVersionInfo(data, ".cat", "catalogue");
		}

		public static byte[] Decompress(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".cat");
		}

		public static XmlDocument DecompressXml(byte[] data)
		{
			return CompressionUtils.DecompressXmlDocumentFromZip(data, ".cat");
		}

		public static XmlDocument DecompressXml(string path)
		{
			return CompressionUtils.DecompressXmlDocumentFromZipFile(path, ".cat");
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

		/// <summary>
		/// Get a list of catalogues referenced by this catalogue. This returns an empty list if none found.
		/// </summary>
		/// <param name="gameSystem">Game System</param>
		/// <returns>List of catalogues</returns>
		public List<Catalogue> GetCatalogues(GameSystem gameSystem)
        {
			// get any catalogue links
			List<Catalogue> catalogues = new List<Catalogue>();
			foreach (CatalogueLink catalogueLink in catalogueLinks)
			{
				Catalogue linkedCatalogue = SystemManager.Instance.GetCatalogueById(gameSystem, catalogueLink.targetId);
				if (linkedCatalogue != null && !catalogues.Contains(linkedCatalogue))
				{
					catalogues.Add(linkedCatalogue);
				}
			}
			return catalogues;
		}
	}
}
