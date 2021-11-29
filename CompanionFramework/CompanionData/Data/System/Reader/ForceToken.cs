using Companion.Data.Utils;
using System.Collections.Generic;
using System.Text;

namespace Companion.Data
{
	public enum ForceReaderState
	{
		Name,
		Cost,
		Faction,
		CostSummary,
	}

	public class ForceToken
	{
		public readonly string name;
		public readonly string cost;
		public readonly string faction;

		public readonly string[] costSummary;

		public ForceToken(string name, string cost, string faction, string[] costSummary)
		{
			this.name = name;
			this.cost = cost;
			this.faction = faction;
			this.costSummary = costSummary;
		}

		public static ForceToken ParseToken(RosterToken rosterToken)
		{
			if (rosterToken == null || rosterToken.tokenType != RosterTokenType.Force)
				return null;

			string cost = null;
			string name = null;
			string faction = null;

			List<string> costSummary = new List<string>();

			// strip leading + and spaces
			string text = ReaderUtils.StripFormatting(rosterToken.content);

			WordReader wordReader = new WordReader(text);

			ForceReaderState state = ForceReaderState.Name;

			// read all words
			string word;
			StringBuilder builder = new StringBuilder();
			while ((word = wordReader.ReadWord()) != null)
			{
				if (state == ForceReaderState.Name)
				{
					if (ReaderUtils.IsCost(word))
					{
						// store name
						name = builder.ToString();

						// read faction next and store
						state = ForceReaderState.Faction;
						cost = word;

						// reset builder for faction name
						builder.Clear();
						continue;
					}

					if (builder.Length > 0)
						builder.Append(" ");

					builder.Append(word);
				}
				else if (state == ForceReaderState.Faction)
				{
					if (builder.Length == 0)
					{
						if (!word.StartsWith("("))
						{
							break;
						}
						else if (word.EndsWith(")"))
						{
							if (builder.Length > 0)
								builder.Append(" ");

							builder.Append(word.Substring(1, word.Length - 2));

							// store
							faction = builder.ToString();
							state = ForceReaderState.CostSummary;

							builder.Clear();
						}
						else
						{
							builder.Append(word.Substring(1));
						}	
					}
					else if (word.EndsWith(")"))
					{
						if (builder.Length > 0)
							builder.Append(" ");

						builder.Append(word.Substring(0, word.Length - 1));

						// store
						faction = builder.ToString();
						state = ForceReaderState.CostSummary;

						builder.Clear();
					}
					else
					{
						if (builder.Length > 0)
							builder.Append(" ");

						builder.Append(word);
					}
				}
				else if (state == ForceReaderState.CostSummary)
				{
					if (costSummary.Count == 0 && builder.Length == 0)
					{
						if (!word.StartsWith("["))
						{
							break;
						}
						else
						{
							if (word.EndsWith(","))
							{
								costSummary.Add(word.Substring(1, word.Length - 2)); // just add straight away
							}
							else if (word.EndsWith("]"))
							{
								costSummary.Add(word.Substring(1, word.Length - 2)); // just add straight away
								break; // we are done
							}
							else
							{
								builder.Append(word.Substring(1));
							}
						}
					}
					else if (word.EndsWith(","))
					{
						if (builder.Length > 0)
							builder.Append(" ");

						builder.Append(word.Substring(0, word.Length - 1));

						// add to costs
						string costString = builder.ToString();
						costSummary.Add(costString);

						builder.Clear();
					}
					else if (word.EndsWith("]"))
					{
						if (builder.Length > 0)
							builder.Append(" ");

						builder.Append(word.Substring(0, word.Length - 1));

						// add to costs
						string costString = builder.ToString();
						costSummary.Add(costString);

						// we are done
						break;
					}
					else
					{
						if (builder.Length > 0)
							builder.Append(" ");

						builder.Append(word);
					}					
				}
			}

			return new ForceToken(name, cost, faction, costSummary.ToArray());
		}
	}
}
