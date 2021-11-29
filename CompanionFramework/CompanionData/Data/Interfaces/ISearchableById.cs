namespace Companion.Data
{
	public interface ISearchableById
	{
		XmlData SearchById(string id);
	}

	public interface ISearchableById<T> where T : IIdentifiable
	{
		T SearchById(string id);
	}
}
