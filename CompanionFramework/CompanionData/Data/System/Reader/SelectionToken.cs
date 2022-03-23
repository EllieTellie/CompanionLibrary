using Companion.Data.Utils;
using System.Collections.Generic;

namespace Companion.Data
{
	public class SelectionToken
	{
		public readonly string name;

		public readonly string[] costSummary;
		public readonly string[] entries;

		//public readonly List<SelectionToken> selections = new List<SelectionToken>();

		public SelectionToken(string name, string[] costSummary, string[] entries)
		{
			this.name = name;
			this.costSummary = costSummary;
			this.entries = entries;
		}

		//public void AddSelection(SelectionToken selection)
		//{
		//	selections.Add(selection);
		//}

		public static SelectionToken ParseToken(RosterToken rosterToken)
		{
			if (rosterToken == null || rosterToken.tokenType != RosterTokenType.Selection)
				return null;

			// strip leading + and spaces
			string text = ReaderUtils.StripFormatting(rosterToken.content);

			int entriesIndex = ReaderUtils.FindSelectionSeparator(text); // this version can find Strategem: Relics of the Chapter [-1CP]: Number of Extra Relics
			if (entriesIndex > 0)
			{
				string nameSplit = text.Substring(0, entriesIndex);

				string entriesSplit = text.Substring(entriesIndex + 1);
				string[] entries = entriesSplit.Split(',');

				//entries.Trim(); // trim leading and trailing, this is not desired as for some reason a lot of names may have a trailing space for some reason
				entries.TrimStart(); // trim leading only, see above

				List<string> costs = new List<string>();
				string name = ReaderUtils.StripCosts(nameSplit, costs);

				return new SelectionToken(name, costs.ToArray(), entries);
			}
			else
			{
				List<string> costs = new List<string>();
				string name = ReaderUtils.StripCosts(text, costs);

				return new SelectionToken(name, costs.ToArray(), null);
			}
		}
	}
}
