using System.Collections.Generic;

namespace Companion.Data.Utils
{
	public static class SortingUtils
	{
		public static void MergeSelections(List<Selection> selections, params string[] ignoreTypes)
		{
			List<Selection> selectionsToRemove = new List<Selection>();
			for (int i = 0; i < selections.Count; i++)
			{
				Selection selection = selections[i];

				if (selectionsToRemove.Contains(selection))
					continue;

				List<Selection> matchingSelections = GetSelectionsByEntryId(selections, selection.entryId, selection);

				foreach (Selection match in matchingSelections)
				{
					if (selectionsToRemove.Contains(match))
						continue;

					if (selection.IsSame(match) && !Contains(ignoreTypes, selection.type))
					{
						selectionsToRemove.Add(match);
						selection.number += match.number;
					}
				}
			}

			foreach (Selection remove in selectionsToRemove)
			{
				selections.Remove(remove);
			}
		}

		private static bool Contains(string[] ignoreTypes, string type)
		{
			if (ignoreTypes == null)
				return false;

			foreach (string ignoreType in ignoreTypes)
			{
				if (type == ignoreType)
					return true;
			}

			return false;
		}

		private static List<Selection> GetSelectionsByEntryId(List<Selection> selections, string entryId, Selection excluded = null)
		{
			List<Selection> foundSelections = new List<Selection>();

			foreach (Selection selection in selections)
			{
				if (excluded != null && selection == excluded)
					continue;

				if (selection.entryId == entryId)
					foundSelections.Add(selection);
			}

			return foundSelections;
		}

		public static int UnitSort(Selection x, Selection y)
		{
			if (x == y)
				return 0;
			else if (x == null)
				return -1;
			else if (y == null)
				return 1;

			Category categoryX = x.GetPrimaryCategory();
			Category categoryY = y.GetPrimaryCategory();

			if (categoryX == categoryY)
				return 0;
			else if (categoryX == null)
				return -1;
			else if (categoryY == null)
				return 1;

			return categoryX.name.CompareTo(categoryY.name);
		}

		public static int UnitSortByCategoryOrder(Selection x, Selection y)
		{
			if (x == y)
				return 0;
			else if (x == null)
				return -1;
			else if (y == null)
				return 1;

			Category categoryX = x.GetPrimaryCategory();
			Category categoryY = y.GetPrimaryCategory();

			if (categoryX == categoryY)
				return 0;
			else if (categoryX == null)
				return -1;
			else if (categoryY == null)
				return 1;

			return categoryX.GetCategoryOrder().CompareTo(categoryY.GetCategoryOrder());
		}

		public static int CategoriesByPrimary(Category x, Category y)
		{
			if (x == y)
				return 0;
			else if (x == null)
				return -1;
			else if (y == null)
				return 1;

			if (y.primary == x.primary)
				return x.name.CompareTo(y.name);

			return y.primary.CompareTo(x.primary);
		}
	}
}
