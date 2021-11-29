using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class GameSystem : XmlData, IIdentifiable, INameable
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

		public GameSystem(XmlNode node) : base(node)
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

			XmlNode readmeNode = node.GetNode("readme");
			readme = readmeNode != null ? readmeNode.InnerText : null;

			publications = ParseXmlList<Publication>(node.GetNodesFromPath("publications", "publication"));
			costTypes = ParseXmlList<CostType>(node.GetNodesFromPath("costTypes", "costType"));
			profileTypes = ParseXmlList<ProfileType>(node.GetNodesFromPath("profileTypes", "profileType"));
			categoryEntries = ParseXmlList<CategoryEntry>(node.GetNodesFromPath("categoryEntries", "categoryEntry"));
			forceEntries = ParseXmlList<ForceEntry>(node.GetNodesFromPath("forceEntries", "forceEntry"));
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"));
			sharedSelectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("sharedSelectionEntries", "selectionEntry"));
			sharedSelectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("sharedSelectionEntryGroups", "selectionEntryGroup"));
			sharedRules = ParseXmlList<Rule>(node.GetNodesFromPath("sharedRules", "rule"));
			sharedProfiles = ParseXmlList<Profile>(node.GetNodesFromPath("sharedProfiles", "profile"));
		}

		public static GameSystem LoadGameSystem(string path)
		{
			byte[] data = FileUtils.ReadFileSimple(path);

			// unzip data
			byte[] uncompressedData = Decompress(data);

			string text = FileUtils.GetString(uncompressedData);

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(text);

			return new GameSystem(xmlDocument.GetNode("gameSystem"));
		}

		public static byte[] Decompress(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".gst");
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