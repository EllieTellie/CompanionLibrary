using System.Collections.Generic;

namespace Companion.Data
{
	public interface ISelectionEntryContainer
	{
		List<SelectionEntry> GetSelectionEntries(GameSystem gameSystem);
		SelectionEntry GetSelectionEntryByName(GameSystem gameSystem, string name, bool contains = false, SelectionEntry excludedEntry = null);
		SelectionResult GetSelectionEntryByName(GameSystemGroup gameSystemGroup, string name, bool contains = false, SelectionEntry excludedEntry = null);
	}
}
