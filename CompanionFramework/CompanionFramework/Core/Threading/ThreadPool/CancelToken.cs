using System;

namespace CompanionFramework.Core.Threading.ThreadPool
{
	/// <summary>
	/// Class used to cancel threaded operations thread safe.
	/// </summary>
	public class CancelToken
	{
		private readonly object lockObject = new Object();
		private bool cancelRequested = false;

		public bool IsCancelRequest
		{
			get { lock (lockObject) { return cancelRequested; } }
		}

		public void Cancel()
		{
			lock (lockObject)
			{
				cancelRequested = true;
			}
		}

		public void AttemptThrowCancelException()
		{
			if (IsCancelRequest)
				throw new OperationCanceledException();
		}
	}
}