public delegate void UpdateEventComplete(object result);
public delegate void UpdateEventAborted(bool critical);

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Handles updating the data files.
	/// </summary>
	public interface IUpdateProcess
	{
		/// <summary>
		/// Fired when the process completed.
		/// </summary>
		event UpdateEventComplete LoadingComplete;

		/// <summary>
		/// Fired when the process is aborted.
		/// </summary>
		event UpdateEventAborted LoadingAborted;

		/// <summary>
		/// Execute the update process and passes any state through.
		/// </summary>
		/// <param name="state">current state</param>
		void Execute(UpdateStateData state);

		/// <summary>
		/// Aborts the process.
		/// </summary>
		void Abort();

		/// <summary>
		/// The update state this process handles.
		/// </summary>
		/// <returns>update state</returns>
		UpdateState GetState();
	}
}
