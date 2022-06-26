using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class SelectionEntryGroup : XmlData, INameable, IIdentifiable, ISelectionEntryContainer
	{
		public string id;
		public string name;
		public bool hidden;
		public bool collective;
		public bool import;

		public List<Modifier> modifiers;
		public List<Constraint> constraints;
		public List<EntryLink> entryLinks;
		public List<SelectionEntryGroup> selectionEntryGroups;
		public List<SelectionEntry> selectionEntries;
		public List<Profile> profiles;

		public SelectionEntryGroup(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");
			collective = node.GetAttributeBool("collective");
			import = node.GetAttributeBool("import");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			constraints = ParseXmlList<Constraint>(node.GetNodesFromPath("constraints", "constraint"));
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"));
			selectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("selectionEntries", "selectionEntry"));
			selectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("selectionEntryGroups", "selectionEntryGroup"));
			profiles = ParseXmlList<Profile>(node.GetNodesFromPath("profiles", "profile"));
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		public List<SelectionEntry> GetSelectionEntries(GameSystem gameSystem)
		{
			SystemManager manager = SystemManager.Instance;

			List<SelectionEntry> selectionEntries = new List<SelectionEntry>();

			selectionEntries.AddRange(selectionEntries);

			foreach (EntryLink entryLink in entryLinks)
			{
				List<EntryLink> entryLinks = manager.GetEntryLinks(gameSystem, entryLink.targetId);
				List<SelectionEntry> targets = entryLinks.GetTargets<SelectionEntry>(gameSystem);

				selectionEntries.AddRange(targets);
			}

			foreach (SelectionEntryGroup entryGroup in selectionEntryGroups)
			{
				List<SelectionEntry> entries = entryGroup.GetSelectionEntries(gameSystem);
				selectionEntries.AddRange(entries);
			}

			return selectionEntries;
		}

		public SelectionEntry GetSelectionEntryByName(GameSystem gameSystem, string name, bool contains = false, SelectionEntry excludedEntry = null)
		{
			foreach (SelectionEntry selectionEntry in selectionEntries)
			{
				SelectionEntry entry = selectionEntry.GetSelectionEntryByName(gameSystem, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			foreach (SelectionEntryGroup entryGroup in selectionEntryGroups)
			{
				SelectionEntry entry = entryGroup.GetSelectionEntryByName(gameSystem, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			foreach (EntryLink entryLink in entryLinks)
			{
				SelectionEntry entry = entryLink.GetSelectionEntryByName(gameSystem, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			return null;
		}

		public SelectionResult GetSelectionEntryByName(GameSystemGroup gameSystemGroup, string name, bool contains = false, SelectionEntry excludedEntry = null)
		{
			foreach (SelectionEntry selectionEntry in selectionEntries)
			{
				SelectionResult entry = selectionEntry.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null && entry.selectionEntry != null)
					return entry;
			}

			foreach (SelectionEntryGroup entryGroup in selectionEntryGroups)
			{
				SelectionResult entry = entryGroup.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null && entry.selectionEntry != null)
					return entry;
			}

			foreach (EntryLink entryLink in entryLinks)
			{
				SelectionResult entry = entryLink.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null && entry.selectionEntry != null)
					return entry;
			}

			return null;
		}
	}
}
