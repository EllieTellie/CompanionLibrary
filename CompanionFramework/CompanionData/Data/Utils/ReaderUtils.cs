using System;
using System.Collections.Generic;
using System.Text;

namespace Companion.Data.Utils
{
	public static class ReaderUtils
	{
		public static bool IsWhiteSpace(string line)
		{
			for (int i = 0; i < line.Length; i++)
			{
				if (!char.IsWhiteSpace(line[i]))
					return false;
			}

			return true;
		}

		public static int GetSelectionNumberFromName(string name, out string strippedName)
		{
			StringBuilder builder = new StringBuilder();

			int end = 0;
			bool success = false;
			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				if (char.IsNumber(c))
				{
					end = i;
					builder.Append(c);
				}
				else if (builder.Length > 0 && c == 'x')
				{
					success = true;

					// if next character is a space also include it
					if ((i + 1) < name.Length && name[i + 1] == ' ')
						end = i + 1;
					else
						end = i;
					break;
				}
				else
				{
					break;
				}
			}

			if (builder.Length == 0 || !success)
			{
				strippedName = name;
				return 1; // default to 1
			}
			else
			{
				strippedName = name.Substring(end + 1);
				string numberText = builder.ToString();

				return int.Parse(numberText);
			}
		}

		public static int FindSelectionSeparator(string text)
		{
			int entriesIndex = text.IndexOf(": ");

			if (entriesIndex >= 0)
            {
				int nextEntriesIndex = entriesIndex + 1 < text.Length ? text.IndexOf(": ", entriesIndex + 1) : -1;

				// handle case for: Strategem: Relics of the Chapter [-1CP]: Number of Extra Relics
				// handle case for: Overlord [6 PL, 140pts]: Relic: Orb of Eternity
				// handle case for: Technomancer: Arkana: Phylacterine Hive, Canoptek Cloak - > "1. Technomancer 2. Arkana: Phylacterine Hive etc"
				// which has multiple ":"
				if (nextEntriesIndex >= 0) // we could while loop this as well if we need to
                {
					bool valid = false;
					int bracketCount = 0;

					// check if there's any commas in between as that means it is not valid to advance to the next ":"
					for (int i=entriesIndex; i<nextEntriesIndex; i++)
                    {
						char c = text[i];
	
						if (c == ',' && bracketCount == 0) // it's valid if we are in any [] cost block
                        {
							valid = false;
							break;
                        }
						else if (c == '[')
                        {
							valid = true; // just allow it for selections that have costs, but not for subselections which don't have costs
							bracketCount++;
                        }
						else if (c == ']')
                        {
							bracketCount--;
                        }
                    }

					if (valid)
                    {
						// advance index
						entriesIndex = nextEntriesIndex;
                    }
                }
			}

			return entriesIndex;
		}

        public static int GetSelectionIndex(string text)
		{
			if (string.IsNullOrEmpty(text) || text.Length < 2)
				return 0;

			int index = 0;

			int textIndex = 0;
			string indexText = text.Substring(textIndex, 2);
			while (indexText == ". ")
			{
				index++;

				textIndex += 2;

				// check for end of text
				if (textIndex + 2 >= text.Length)
					return index;

				// keep going
				indexText = text.Substring(textIndex, 2);
			}

			return index;
		}

		public static bool IsCost(string text)
		{
			bool hasNumber = false;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if (char.IsWhiteSpace(c))
					continue;

				if (!hasNumber)
				{
					if (c == '-') // followed by number
					{
						continue;
					}
					else if (char.IsNumber(c))
					{
						hasNumber = true;
					}
					else
					{
						return false; // no numbers
					}
				}
				else
				{
					if (char.IsLetterOrDigit(c)) // just assume yes?
						return true;
				}
			}

			return false;
		}

		public static string StripCosts(string text, List<string> costs)
		{
			int startIndex = text.IndexOf('[');

			if (startIndex < 0)
				return text;

			string name = text.Substring(0, startIndex - 1);

			int endIndex = text.IndexOf(']', startIndex + 1);
			string costText = text.Substring(startIndex + 1, endIndex - startIndex - 1);

			string[] costsSplit = costText.Split(',');
			foreach (string s in costsSplit)
			{
				costs.Add(s.Trim());
			}

			return name;
		}

		public static string StripFormatting(string text)
		{
			int start = 0;
			int end = text.Length;

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if (c == '+' || c == '.' || char.IsWhiteSpace(c))
				{
					start++;
				}
				else
				{
					break;
				}
			}

			if (start > 0)
			{
				for (int i = text.Length - 1; i >= 0; i--)
				{
					char c = text[i];

					if (c == '+' || c == '.' || char.IsWhiteSpace(c))
					{
						end--;
					}
					else
					{
						break;
					}
				}
			}

			return text.Substring(start, end - start);
		}
	}
}
