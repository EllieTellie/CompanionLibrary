using CompanionFramework.Net.Http.Common;
using System;

namespace CompanionFramework.Net.Http
{
	public class HttpDownloadProgress : IStreamProgress
	{
		public readonly HttpDownload download;

		/// <summary>
		/// Fire download update delegate. Source is <see cref="HttpDownloadProgress"/>.
		/// </summary>
		public event EventHandler OnDownloadUpdate;

		public long bytesReceived;
		public long totalBytes;

		/// <summary>
		/// Create a new progress class that tracks the progress for the download. This registers itself with the download automatically.
		/// </summary>
		/// <param name="download">Download to track progress</param>
		public HttpDownloadProgress(HttpDownload download)
		{
			this.download = download;

			// register automatically
			download.SetProgressTracker(this);
		}

		public void UpdateProgress(long bytesReceived, long totalBytes)
		{
			this.bytesReceived = bytesReceived;
			this.totalBytes = totalBytes;

			if (OnDownloadUpdate != null)
				OnDownloadUpdate(this, null);
		}
	}
}
