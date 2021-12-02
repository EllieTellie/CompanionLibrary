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
		/// Execute the loading process and passes any state through.
		/// </summary>
		/// <param name="state">current state</param>
		public abstract void Execute(UpdateStateData state);

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
			Cleanup();

			if (LoadingAborted != null)
				LoadingAborted(false);
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
		public void Retry(UpdateStateData state)
		{
			Execute(state);
		}
	}
}
