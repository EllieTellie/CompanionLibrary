namespace Companion.Data
{
	/// <summary>
	/// Simple reader that reads words. 
	/// </summary>
	public class WordReader
	{
		protected readonly string text;
		protected int position = 0;

		public WordReader(string text)
		{
			this.text = text;
		}

		public string ReadWord()
		{
			if (position > 0 && position >= text.Length)
			{
				return null;
			}
			else if (position == 0 && text.Length == 0)
			{
				position = 1; // advance to complete
				return null;
			}

			int mark = -1;
			for (int charIndex = position; charIndex < text.Length; charIndex++)
			{
				char c = text[charIndex];

				if (mark < 0)
				{
					if (!char.IsWhiteSpace(c))
					{
						mark = charIndex;
					}
				}
				else if (char.IsWhiteSpace(c))
				{
					position = charIndex;
					return text.Substring(mark, charIndex - mark);
				}
			}

			// if we found something but are still reading then just read until the end
			if (mark >= 0)
			{
				position = text.Length;
				return text.Substring(mark, text.Length - mark);
			}

			return null;
		}
	}
}
