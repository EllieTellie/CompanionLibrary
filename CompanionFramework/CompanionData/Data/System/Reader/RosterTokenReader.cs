using Companion.Data.Utils;
using System.Collections.Generic;
using System.IO;

namespace Companion.Data
{
	public enum ReaderState
	{
		None,
		Force,
		Selection,
		Completed
	}

	public class RosterTokenReader
	{
		protected readonly string text;
		protected StringReader reader;
		protected ReaderState state;

		public RosterTokenReader(string text)
		{
			this.text = text;

			reader = new StringReader(text);
			state = ReaderState.None;
		}

		public List<RosterToken> ReadAllTokens()
		{
			List<RosterToken> tokens = new List<RosterToken>();

			RosterToken token;

			while ((token = ReadRosterToken()) != null)
			{
				tokens.Add(token);
			}

			// reset
			Reset();

			return tokens;
		}

		public RosterToken ReadRosterToken()
		{
			string line = reader.ReadLine();

			while (line != null)
			{
				// check if line contains something
				if (line == string.Empty || ReaderUtils.IsWhiteSpace(line))
				{
					line = reader.ReadLine(); // keep reading
					continue;
				}

				if (state == ReaderState.Completed)
				{
					return new RosterToken(RosterTokenType.Comment, line);
				}

				RosterTokenType tokenType = DetectTokenType(line);
				if (tokenType == RosterTokenType.Invalid)
				{
					line = reader.ReadLine(); // keep reading
					continue;
				}
				
				if (tokenType == RosterTokenType.Summary)
				{
					state = ReaderState.Completed;
				}

				return new RosterToken(tokenType, line);
			}

			return null;
		}

		private RosterTokenType DetectTokenType(string line)
		{
			if (line.StartsWith("++"))
			{
				if (line.StartsWith("++ Total:"))
					return RosterTokenType.Summary;
				else
					return RosterTokenType.Force;
			}
			else if (line.StartsWith("+"))
			{
				return RosterTokenType.Category;
			}
			else
			{
				return RosterTokenType.Selection;
			}
		}

		public void Reset()
		{
			reader = new StringReader(text);
			state = ReaderState.None;
		}
	}
}
