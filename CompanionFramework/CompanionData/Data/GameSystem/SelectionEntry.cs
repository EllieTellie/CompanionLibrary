using CompanionFramework.Core.Log;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class SelectionEntry : XmlData, INameable, IIdentifiable, ISelectionEntryContainer
	{
		public string id;
		public string name;
		public string publicationId;
		public string page; // can be multiple pages, so cannot be an integer
		public bool hidden;
		public bool collective;
		public bool import;
		public string type;

		public List<Modifier> modifiers;
		public List<ModifierGroup> modifierGroups;
		public List<Constraint> constraints;
		public List<InfoLink> infoLinks;
		public List<InfoGroup> infoGroups;
		public List<EntryLink> entryLinks;
		public List<CategoryLink> categoryLinks;
		public List<SelectionEntry> selectionEntries;
		public List<SelectionEntryGroup> selectionEntryGroups;
		public List<Cost> costs;
		public List<Rule> rules;
		public List<Profile> profiles;

		/// <summary>
		/// Parent selection if available.
		/// </summary>
		protected SelectionEntry parent;

		public SelectionEntry(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");

			publicationId = node.GetAttribute("publicationId");
			page = node.GetAttribute("page");
			hidden = node.GetAttributeBool("hidden");
			collective = node.GetAttributeBool("collective");
			import = node.GetAttributeBool("import");
			type = node.GetAttribute("type");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			modifierGroups = ParseXmlList<ModifierGroup>(node.GetNodesFromPath("modifierGroups", "modifierGroup"));
			constraints = ParseXmlList<Constraint>(node.GetNodesFromPath("constraints", "constraint"));
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"));
			infoLinks = ParseXmlList<InfoLink>(node.GetNodesFromPath("infoLinks", "infoLink"));
			infoGroups = ParseXmlList<InfoGroup>(node.GetNodesFromPath("infoGroups", "infoGroup"));
			categoryLinks = ParseXmlList<CategoryLink>(node.GetNodesFromPath("categoryLinks", "categoryLink"));
			selectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("selectionEntries", "selectionEntry"));
			selectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("selectionEntryGroups", "selectionEntryGroup"));
			costs = ParseXmlList<Cost>(node.GetNodesFromPath("costs", "cost"));
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"));
			profiles = ParseXmlList<Profile>(node.GetNodesFromPath("profiles", "profile"));

			SetupParent();
		}

		private void SetupParent()
		{
			foreach (SelectionEntry selection in selectionEntries)
			{
				selection.SetParent(this);
			}
		}

		public SelectionEntry GetParent()
		{
			return parent;
		}

		public SelectionEntry GetRootSelection()
		{
			if (parent != null)
			{
				return parent.GetRootSelection();
			}
			else
			{
				return this;
			}
		}

		private void SetParent(SelectionEntry selectionEntry)
		{
			this.parent = selectionEntry;
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		public override string ToString()
		{
			return name + ": " + id;
		}

		public List<SelectionEntry> GetSelectionEntries(GameSystem gameSystem)
		{
			List<SelectionEntry> selectionEntries = new List<SelectionEntry>();
			selectionEntries.AddRange(selectionEntries);

			foreach (EntryLink entryLink in entryLinks)
			{
				List<SelectionEntry> entries = entryLink.GetSelectionEntries(gameSystem);
				selectionEntries.AddRange(entries);
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
			if (this.name == name || (contains && this.name.Contains(name)))
			{
				if (this != excludedEntry)
					return this;
			}

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
			if (this.name == name || (contains && this.name.Contains(name)))
			{
				if (this != excludedEntry)
					return new SelectionResult(this);
			}

			foreach (SelectionEntry selectionEntry in selectionEntries)
			{
				SelectionResult entry = selectionEntry.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			foreach (SelectionEntryGroup entryGroup in selectionEntryGroups)
			{
				SelectionResult entry = entryGroup.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			foreach (EntryLink entryLink in entryLinks)
			{
				SelectionResult entry = entryLink.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			return null;
		}
	}
}
