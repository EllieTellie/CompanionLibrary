using CompanionFramework.Core.Log;

namespace Companion.Data.System.Update
{
	public abstract class CoreUpdateProcess : IUpdateProcess
	{
		/// <summary>
		/// Fired when the process completed.
		/// </summary>
		public event UpdateEventComplete LoadingComplete;

		/// <summary>
		/// Fired when the process is aborted.
		/// </summary>
		public event UpdateEventAborted LoadingAborted;

		/// <summary>
		/// Execute the update process and passes any state through.
		/// </summary>
		/// <param name="state">current state</param>
		public abstract void Execute(RepositoryData state);

		/// <summary>
		/// The update state this process handles.
		/// </summary>
		/// <returns>update state</returns>
		public abstract UpdateState GetState();

		/// <summary>
		/// Aborts the process. Fires the abort event.
		/// </summary>
		public virtual void Abort()
		{
			Abort(UpdateError.Error, null); // abort with default error and no message
		}

		/// <summary>
		/// Aborts the process. Fires the abort event.
		/// </summary>
		public virtual void Abort(UpdateError error, string message = null)
		{
			Cleanup();

			// automatically log error message
			if (message != null)
				FrameworkLogger.Error(message);

			if (LoadingAborted != null)
				LoadingAborted(error, message);
		}

		/// <summary>
		/// Complete the process. Fires the complete event.
		/// </summary>
		protected virtual void Complete()
		{
			Cleanup();

			if (LoadingComplete != null)
				LoadingComplete(null);
		}

		/// <summary>
		/// Complete the process. Fires the complete event.
		/// </summary>
		protected virtual void Complete(object result)
		{
			Cleanup();

			if (LoadingComplete != null)
				LoadingComplete(result);
		}

		/// <summary>
		/// Cleanup. Fired from Abort() and Complete(). By default does nothing.
		/// </summary>
		protected virtual void Cleanup()
		{
		}

		/// <summary>
		/// How many times it should retry. By default it does not.
		/// </summary>
		/// <returns>Maximum retries</returns>
		public virtual int GetMaxRetries()
		{
			return 0;
		}

		/// <summary>
		/// Retry the operation.
		/// </summary>
		/// <param name="state">State</param>
		public void Retry(RepositoryData state)
		{
			Execute(state);
		}
	}
}
