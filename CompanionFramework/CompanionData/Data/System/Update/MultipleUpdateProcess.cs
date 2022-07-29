using CompanionFramework.Core.Log;
using System;
using System.Collections.Generic;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Multiple update process.
	/// </summary>
    public class MultipleUpdateProcess : IUpdateProcess
    {
        /// <summary>
		/// Fired when the process completed.
		/// </summary>
		public event UpdateEventComplete LoadingComplete;

        /// <summary>
        /// Fired when the process is aborted.
        /// </summary>
        public event UpdateEventAborted LoadingAborted;

        protected List<IUpdateProcess> processes;
        protected RepositoryData state;

		protected int processIndex;

        public MultipleUpdateProcess(params IUpdateProcess[] processes)
        {
			if (processes != null)
				this.processes = new List<IUpdateProcess>(processes);
        }

        public void AddProcess(IUpdateProcess process)
        {
			processes.Add(process);
		}

        public void Execute(RepositoryData state)
        {
            this.state = state;

            ExecuteNext();
        }

        private void ExecuteNext()
        {
			if (processes.Count == 0)
            {
				Complete();
				return;
            }

			// process
			try
			{
				IUpdateProcess process = processes[0];

				process.LoadingComplete += OnProcessComplete;
				process.LoadingAborted += OnProcessFailed;

				process.Execute(state);
			}
			catch (Exception e)
			{
				IUpdateProcess module = processes[0];
				Abort(UpdateError.Error, "Process Exception: " + e.Message + " Type: " + module.GetType().Name);
				return;
			}
		}

        private void OnProcessComplete(object result)
        {
			IUpdateProcess process = processes[0];

			// remove events
			process.LoadingComplete -= OnProcessComplete;
			process.LoadingAborted -= OnProcessFailed;

			// remove
			processes.RemoveAt(0);

			// go next
			ExecuteNext();
		}

        private void OnProcessFailed(UpdateError error, string message)
        {
			IUpdateProcess process = processes[0];

			// remove events
			process.LoadingComplete -= OnProcessComplete;
			process.LoadingAborted -= OnProcessFailed;

			Abort(error, message);
		}

        /// <summary>
        /// Aborts the process. Fires the abort event.
        /// </summary>
        public void Abort()
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
			state = null;
		}
	}
}
