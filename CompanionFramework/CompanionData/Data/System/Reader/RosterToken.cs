namespace Companion.Data
{
	public class RosterToken
	{
		public readonly RosterTokenType tokenType;
		public readonly string content;

		public RosterToken(RosterTokenType tokenType, string content)
		{
			this.tokenType = tokenType;
			this.content = content;
		}
	}
}
