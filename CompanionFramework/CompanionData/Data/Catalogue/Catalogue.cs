using CompanionFramework.IO.Utils;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class Catalogue : XmlData, INameable, IIdentifiable
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

		public Catalogue(XmlNode node) : base(node)
		{
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

			publications = ParseXmlList<Publication>(node.GetNodesFromPath("publications", "publication"));
			profileTypes = ParseXmlList<ProfileType>(node.GetNodesFromPath("profileTypes", "profileType"));
			categoryEntries = ParseXmlList<CategoryEntry>(node.GetNodesFromPath("categoryEntries", "categoryEntry"));
			selectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("selectionEntries", "selectionEntry"));
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"));
			sharedSelectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("sharedSelectionEntries", "selectionEntry"));
			sharedSelectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("sharedSelectionEntryGroups", "selectionEntryGroup"));
			sharedRules = ParseXmlList<Rule>(node.GetNodesFromPath("sharedRules", "rule"));
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"));
			sharedProfiles = ParseXmlList<Profile>(node.GetNodesFromPath("sharedProfiles", "profile"));
			catalogueLinks = ParseXmlList<CatalogueLink>(node.GetNodesFromPath("catalogueLinks", "catalogueLink"));
			sharedInfoGroups = ParseXmlList<InfoGroup>(node.GetNodesFromPath("sharedInfoGroups", "infoGroup"));
			infoLinks = ParseXmlList<InfoLink>(node.GetNodesFromPath("infoLinks", "infoLink"));
			costTypes = ParseXmlList<CostType>(node.GetNodesFromPath("costTypes", "costType"));
			forceEntries = ParseXmlList<ForceEntry>(node.GetNodesFromPath("forceEntries", "forceEntry"));
		}

		public static Catalogue LoadCatalogue(string path)
		{
			byte[] data = FileUtils.ReadFileSimple(path);

			// unzip data
			byte[] uncompressedData = Decompress(data);

			string text = FileUtils.GetString(uncompressedData);

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(text);

			return new Catalogue(xmlDocument.GetNode("catalogue"));
		}

		public static byte[] Decompress(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".cat");
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}
	}
}
