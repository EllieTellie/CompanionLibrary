using CompanionFramework.Net.Http;
using System;

namespace Companion.Data
{
	/// <summary>
	/// Track updating the repository.
	/// </summary>
	public class RepositoryUpdate
	{
		#region Events
		/// <summary>
		/// Fired when a download reports progress. This is most likely fired on whatever thread this is called from
		/// </summary>
		public event Action<HttpDownloadSet, HttpDownloadProgress> OnDownloadUpdate;
		#endregion

		public RepositoryUpdate()
		{
		}

		public void FireDownloadUpdateEvent(HttpDownloadSet downloadSet, HttpDownloadProgress progress)
		{
			if (OnDownloadUpdate != null)
				OnDownloadUpdate(downloadSet, progress);
		}
	}
}