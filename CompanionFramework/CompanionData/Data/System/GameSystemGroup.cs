using System;
using System.Collections.Generic;

namespace Companion.Data
{
	/// <summary>
	/// Group of catalogues and systems used in a roster. Mostly for roster detection speed.
	/// </summary>
	public class GameSystemGroup
	{
		public readonly GameSystem gameSystem;
		protected readonly List<Catalogue> catalogues;

		protected List<string> generatedGuids = new List<string>();

		public GameSystemGroup(GameSystem gameSystem)
		{
			this.gameSystem = gameSystem;
			this.catalogues = new List<Catalogue>();
		}

		public GameSystemGroup(GameSystem gameSystem, List<Catalogue> catalogues)
		{
			this.gameSystem = gameSystem;
			this.catalogues = catalogues;
		}

		/// <summary>
		/// Creates a new guid that is unique for this game system and catalogues.
		/// </summary>
		/// <returns>New guid</returns>
		public string CreateGuid()
		{
			string guid = Guid.NewGuid().ToString();

			// roll until we have a unique id
			while (ContainsGuid(guid))
			{
				guid = Guid.NewGuid().ToString();
			}

			// store these in here
			generatedGuids.Add(guid);

			return guid;
		}

		public bool ContainsGuid(string guid)
		{
			if (gameSystem.HasId(guid))
				return true;

			foreach (Catalogue catalogue in catalogues)
			{
				if (catalogue.HasId(guid))
					return true;
			}

			if (generatedGuids.Contains(guid))
				return true;

			return false;
		}

		/// <summary>
		/// Add this catalogue to the group. If the catalogue has any links it will add those as well.
		/// </summary>
		/// <param name="catalogue">Catalogue to add</param>
		public void AddCatalogue(Catalogue catalogue)
		{
			if (!catalogues.Contains(catalogue))
			{
				catalogues.Add(catalogue);

				// get any catalogue links
				List<CatalogueLink> catalogueLinks = catalogue.catalogueLinks;
				foreach (CatalogueLink catalogueLink in catalogueLinks)
				{
					Catalogue linkedCatalogue = SystemManager.Instance.GetCatalogueById(gameSystem, catalogueLink.targetId);
					if (linkedCatalogue != null && !catalogues.Contains(linkedCatalogue))
					{
						catalogues.Add(linkedCatalogue);
					}
				}
			}
		}

        public void ResetCatalogues()
        {
			catalogues.Clear();
        }

        public List<T> SearchAllByName<T>(string name, bool recursive = false) where T : XmlData, INameable
		{
			List<T> results = new List<T>();

			foreach (Catalogue cat in catalogues)
			{
				cat.SearchAllByName<T>(results, name, recursive);
			}

			// search game system last
			gameSystem.SearchAllByName<T>(results, name, recursive);

			return results;
		}

		public XmlData SearchById(string id, bool recursive = false)
		{
			foreach (Catalogue cat in catalogues)
			{
				IIdentifiable identifiable = cat.GetIdentifiable(id);
				if (identifiable != null)
					return (XmlData)identifiable;

				//XmlData catalogueResult = cat.SearchById(id, recursive);
				//if (catalogueResult != null)
				//	return catalogueResult;
			}

			// search game system last
			IIdentifiable result = gameSystem.GetIdentifiable(id);
			if (result != null)
				return (XmlData)result;

			// search game system last
			//XmlData result = gameSystem.SearchById(id, recursive);
			//if (result != null)
			//	return result;

			return null;
		}

		public T SearchById<T>(string id, bool recursive = false) where T : XmlData, IIdentifiable
		{
			List<T> results = SearchAllById<T>(id, recursive);
			return results != null && results.Count > 0 ? results[0] : null;
		}

		public List<T> SearchAllById<T>(string id, bool recursive = false) where T : XmlData, IIdentifiable
		{
			List<T> results = new List<T>();

			foreach (Catalogue cat in catalogues)
			{
				cat.SearchAllById<T>(results, id, recursive);
			}

			// search game system last
			gameSystem.SearchAllById<T>(results, id, recursive);

			return results;
		}

		public SelectionResult GetSelectionEntryByName(string name, bool contains = false, SelectionEntry excludedEntry = null)
		{
			foreach (Catalogue cat in catalogues)
			{
				foreach (EntryLink entryLink in cat.entryLinks)
				{
					SelectionResult entry = entryLink.GetSelectionEntryByName(this, name, contains, excludedEntry);
					if (entry != null)
						return entry;
				}
			}

			// search game system last
			foreach (EntryLink entryLink in gameSystem.entryLinks)
			{
				SelectionResult entry = entryLink.GetSelectionEntryByName(this, name, contains, excludedEntry);
				if (entry != null)
					return entry;
			}

			return null;
		}

		public List<Catalogue> GetCatalogues()
		{
			return catalogues;
		}
	}
}
