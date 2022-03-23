using System;

namespace Companion.Data
{
	public class SelectionResult
	{
		public SelectionEntry selectionEntry;
		public EntryLink entryLink;

		public SelectionResult(SelectionEntry selectionEntry)
		{
			if (selectionEntry == null)
				throw new ArgumentOutOfRangeException();

			this.selectionEntry = selectionEntry;
			this.entryLink = null;
		}

		public SelectionResult(SelectionEntry selectionEntry, EntryLink entryLink)
		{
			this.selectionEntry = selectionEntry;
			this.entryLink = entryLink;
        }

		/// <summary>
		/// Get selection entry by name from the result. This attempts to find it in the entry link first and the selection entry second.
		/// </summary>
		/// <param name="gameSystemGroup">GameSystemGroup</param>
		/// <param name="name">Name</param>
		/// <param name="allowPartialMatches">True if partial matches are allowed</param>
		/// <param name="excludedEntry">Excluded entry</param>
		/// <returns>Selection result or null if not found</returns>
        public SelectionResult GetSelectionEntryByName(GameSystemGroup gameSystemGroup, string name, bool allowPartialMatches = false, SelectionEntry excludedEntry = null)
        {
			if (entryLink != null)
			{
				SelectionResult result = entryLink.GetSelectionEntryByName(gameSystemGroup, name, false, excludedEntry);
				if (result != null)
					return result;
			}

			if (selectionEntry != null)
            {
				SelectionResult result = selectionEntry.GetSelectionEntryByName(gameSystemGroup, name, false, excludedEntry);
				if (result != null)
					return result;
			}

			if (allowPartialMatches) // do this after both above fail
			{
				if (entryLink != null)
				{
					SelectionResult result = entryLink.GetSelectionEntryByName(gameSystemGroup, name, true, excludedEntry);
					if (result != null)
						return result;
				}

				if (selectionEntry != null)
				{
					SelectionResult result = selectionEntry.GetSelectionEntryByName(gameSystemGroup, name, true, excludedEntry);
					if (result != null)
						return result;
				}
			}

			return null;
        }
	}
}
