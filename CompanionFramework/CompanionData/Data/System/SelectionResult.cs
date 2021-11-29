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
	}
}
