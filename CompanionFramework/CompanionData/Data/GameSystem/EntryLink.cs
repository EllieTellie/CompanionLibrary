using System;
using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class EntryLink : XmlData, INameable, IIdentifiable, ISelectionEntryContainer
	{
		public string id;
		public string name;
		public bool hidden;
		public bool collective;
		public bool import;
		public string targetId;
		public string type;
		public string publicationId;
		public string page;

		public List<CategoryLink> categoryLinks;
		public List<Cost> costs;
		public List<Constraint> constraints;
		public List<Modifier> modifiers;
		public List<ModifierGroup> modifierGroups;
		public List<Profile> profiles;
		public List<SelectionEntry> selectionEntries;
		public List<SelectionEntryGroup> selectionEntryGroups;
		public List<EntryLink> entryLinks;
		public List<InfoLink> infoLinks;
		public List<InfoGroup> infoGroups;

		protected XmlData cachedTarget;

		public EntryLink(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");
			collective = node.GetAttributeBool("collective");
			import = node.GetAttributeBool("import");
			targetId = node.GetAttribute("targetId");
			type = node.GetAttribute("type");
			publicationId = node.GetAttribute("publicationId");
			page = node.GetAttribute("page");

			categoryLinks = ParseXmlList<CategoryLink>(node.GetNodesFromPath("categoryLinks", "categoryLink"));
			costs = ParseXmlList<Cost>(node.GetNodesFromPath("costs", "cost"));
			constraints = ParseXmlList<Constraint>(node.GetNodesFromPath("constraints", "constraint"));
			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			modifierGroups = ParseXmlList<ModifierGroup>(node.GetNodesFromPath("modifierGroups", "modifierGroup"));
			profiles = ParseXmlList<Profile>(node.GetNodesFromPath("profiles", "profile"));
			selectionEntries = ParseXmlList<SelectionEntry>(node.GetNodesFromPath("selectionEntries", "selectionEntry"));
			selectionEntryGroups = ParseXmlList<SelectionEntryGroup>(node.GetNodesFromPath("selectionEntryGroups", "selectionEntryGroup"));
			entryLinks = ParseXmlList<EntryLink>(node.GetNodesFromPath("entryLinks", "entryLink"));
			infoLinks = ParseXmlList<InfoLink>(node.GetNodesFromPath("infoLinks", "infoLink"));
			infoGroups = ParseXmlList<InfoGroup>(node.GetNodesFromPath("infoGroups", "infoGroup"));
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

		public bool HasCosts()
		{
			return costs != null && costs.Count > 0;
		}

		public XmlData GetTarget(GameSystem gameSystem)
		{
			if (cachedTarget != null)
				return cachedTarget;

			// old slower method
			//List<XmlData> results = SystemManager.Instance.SearchAllById(gameSystem, targetId, type, true);
			//XmlData target = results != null && results.Count > 0 ? results[0] : null;

			cachedTarget = SystemManager.Instance.SearchById(gameSystem, targetId, true);

			return cachedTarget;
		}

		public XmlData GetTarget(GameSystemGroup gameSystemGroup)
		{
			if (cachedTarget != null)
				return cachedTarget;

			// old slower method
			//List<XmlData> results = SystemManager.Instance.SearchAllById(gameSystem, targetId, type, true);
			//XmlData target = results != null && results.Count > 0 ? results[0] : null;

			cachedTarget = gameSystemGroup.SearchById(targetId, true);

			return cachedTarget;
		}

		public List<SelectionEntry> GetSelectionEntries(GameSystem gameSystem)
		{
			List<SelectionEntry> foundEntries = new List<SelectionEntry>();

			// add our own selections
			foundEntries.AddRange(selectionEntries);

			// add entry groups
			foreach (SelectionEntryGroup entryGroup in selectionEntryGroups)
			{
				List<SelectionEntry> entries = entryGroup.GetSelectionEntries(gameSystem);
				foundEntries.AddRange(entries);
			}

			// add entry links
			foreach (EntryLink entryLink in entryLinks)
			{
				foundEntries.AddRange(entryLink.GetSelectionEntries(gameSystem));
			}

			// add target
			XmlData target = GetTarget(gameSystem);
			List<SelectionEntry> targetEntries = target.GetAll<SelectionEntry>();
			foundEntries.AddRange(targetEntries);

			return foundEntries;
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

			// add target
			XmlData target = GetTarget(gameSystem);
			ISelectionEntryContainer container = target as ISelectionEntryContainer;
			if (container != null)
			{
				SelectionEntry entry = container.GetSelectionEntryByName(gameSystem, name, contains, excludedEntry);
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

			// add target
			XmlData target = GetTarget(gameSystemGroup);
			ISelectionEntryContainer container = target as ISelectionEntryContainer;
			if (container != null)
			{
				SelectionResult entry = container.GetSelectionEntryByName(gameSystemGroup, name, contains, excludedEntry);
				if (entry != null)
				{
					if (entry.entryLink == null)
						return new SelectionResult(entry.selectionEntry, this); // highjack
					else
						return entry;
				}
			}

			return null;
		}
	}
}
