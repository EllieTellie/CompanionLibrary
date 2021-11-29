namespace CompanionFramework.Core.Threading.ThreadPool
{
	/// <summary>
	/// Simple task for use in the thread pool.
	/// </summary>
	public interface IBaseThreadedTask
	{
		/// <summary>
		/// Run the task.
		/// </summary>
		void Run();

		/// <summary>
		/// Attempt to cancel the task.
		/// </summary>
		void Cancel();
	}
}