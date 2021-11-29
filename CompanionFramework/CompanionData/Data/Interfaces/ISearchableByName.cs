namespace Companion.Data
{
	public interface ISearchableByName
	{
		XmlData SearchByName(string name);
	}

	public interface ISearchableByName<T> where T : INameable
	{
		T SearchByName(string name);
	}
}
