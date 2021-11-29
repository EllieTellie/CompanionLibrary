namespace Companion.Data
{
	public interface IRosterLoader
	{
		Roster LoadRoster(RosterManager rosterManager, string text);
	}
}
