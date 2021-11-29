namespace Companion.Data.Utils
{
	public static class CommonTextUtils
	{
		public static string GetSelectionName(Selection selection, Selection parent)
		{
			int number = selection.number > parent.number ? selection.number : parent.number;
			return GetSelectionName(selection.name, number);
		}

		public static string GetSelectionName(Selection selection)
		{
			return GetSelectionName(selection.name, selection.number);
		}

		public static string GetSelectionName(string name, int number)
		{
			return (number > 1 ? number + " x " : "") + name; // + (selection.number > 1 ? " (" + selection.number + ")" : "");
		}
	}
}
