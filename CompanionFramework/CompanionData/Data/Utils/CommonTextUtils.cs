using System;
using System.Collections.Generic;

namespace Companion.Data.Utils
{
	public static class CommonTextUtils
	{
		public static List<string> GetIdChain(string id)
        {
			List<string> chain = new List<string>();

			int start = 0;
			int index = id.IndexOf("::", start);
			if (index < 0)
			{
				chain.Add(id);
				return chain;
			}

			while (index >= 0)
            {
				chain.Add(id.Substring(start, index - start));

				// advance
				start = index + 2;
				index = id.IndexOf("::", start);

				// handle the trailing text if required
				if (index < 0 && id.Length - start > 0)
                {
					chain.Add(id.Substring(start)); // add the last bit
					break;
                }
			}

			return chain;
        }

		public static string GetTargetId(string id)
        {
			int index = id.LastIndexOf("::");
			return id.Substring(index + 2);
		}

		public static string GetSelectionName(Selection selection, Selection parent, bool stripIndexNumber = false)
		{
			int number = parent == null || selection.number > parent.number ? selection.number : parent.number;
			return GetSelectionName(selection.name, number, stripIndexNumber);
		}

		public static string GetSelectionName(Selection selection, bool stripIndexNumber = false)
		{
			return GetSelectionName(selection.name, selection.number, stripIndexNumber);
		}

		public static string GetSelectionName(string name, int number, bool stripIndexNumber = false)
		{
			if (stripIndexNumber)
				name = StripIndexNumber(name);

			return (number > 1 ? number + " x " : "") + name; // + (selection.number > 1 ? " (" + selection.number + ")" : "");
		}

        private static string StripIndexNumber(string name)
        {
			int target = name.IndexOf(") ");

			bool strip = true; 
			if (target > 0)
            {
				for (int i=0; i<target; i++)
                {
					if (!char.IsNumber(name[i]))
                    {
						strip = false;
						break;
                    }
                }
            }
			else
            {
				strip = false;
            }

			if (strip)
            {
				return name.Substring(target + 2);
            }
			else
            {
				return name;
            }
        }
    }
}
