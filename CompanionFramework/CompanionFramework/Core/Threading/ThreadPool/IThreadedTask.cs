namespace CompanionFramework.Core.Threading.ThreadPool
{
	public delegate void TaskEvent(IThreadedTask task);

	/// <summary>
	/// Threaded task with events for when the task starts and finishes.
	/// </summary>
	public interface IThreadedTask : IBaseThreadedTask
	{
		/// <summary>
		/// Fired when the task started.
		/// </summary>
		event TaskEvent TaskStarted;

		/// <summary>
		/// Fired when the task is finished.
		/// </summary>
		event TaskEvent TaskFinished;
	}
}