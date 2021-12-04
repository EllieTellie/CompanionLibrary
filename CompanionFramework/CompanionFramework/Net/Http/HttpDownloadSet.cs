using CompanionFramework.Core.Log;
using CompanionFramework.Core.Threading.Messaging;
using CompanionFramework.Core.Threading.ThreadPool;
using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;

namespace CompanionFramework.Net.Http
{
	/// <summary>
	/// Class to allow downloading multiple files easily.
	/// </summary>
	public class HttpDownloadSet : IBaseThreadedTask
	{
		/// <summary>
		/// Fired when any or all downloads have failed/cancelled. Source is <see cref="HttpDownloadSet"/>. EventArgs is <see cref="HttpDownloadFailureEventArgs"/>.
		/// </summary>
		public event EventHandler DownloadsFailed;

		/// <summary>
		/// Fired when all downloads have completed. Source is <see cref="HttpDownloadSet"/>. EventArgs is null.
		/// </summary>
		public event EventHandler DownloadsCompleted;

		/// <summary>
		/// Fire download update on the main thread. Source is <see cref="HttpDownloadProgress"/>. This only happens when progress tracking is set to true.
		/// </summary>
		public event EventHandler OnDownloadUpdate;

		private HttpDownload[] downloads;
		private CancelToken cancelToken = new CancelToken();

		private readonly bool progressTracking;
		private bool init;

		/// <summary>
		/// Returns a list of files that have been downloaded.
		/// </summary>
		public List<string> DownloadedFiles { get { return downloadedFiles; } }
		private List<string> downloadedFiles;

		public HttpDownloadSet(params HttpDownload[] downloads) : this(false, downloads)
		{
		}

		public HttpDownloadSet(bool progressTracking, params HttpDownload[] downloads)
		{
			if (downloads == null)
				throw new ArgumentNullException();

			this.downloads = downloads;
			this.progressTracking = progressTracking;

			downloadedFiles = new List<string>(1);
        }

		/// <summary>
		/// Attempt to cancel the downloads.
		/// </summary>
		public void Cancel()
		{
			cancelToken.Cancel();
		}

		private bool CancelCheck()
		{
			if (cancelToken.IsCancelRequest)
			{
				Failed(new HttpDownloadFailureEventArgs(HttpDownloadFailure.Cancelled));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Start the downloads.
		/// </summary>
		public void Run()
		{
			if (CancelCheck())
			{
				return;
			}

			if (init)
			{
				throw new InvalidOperationException();
			}

			init = true;
			StartDownloads();	
		}

		// only supposed to be called once
		private void StartDownloads()
		{
			bool started = false;
			foreach (HttpDownload download in downloads)
			{
				if (download == null)
					continue;

				// listen for download
				download.DownloadCompleted += OnDownloadComplete;
				download.DownloadFailed += OnDownloadFailed;

				// track download progress
				if (progressTracking)
				{
					HttpDownloadProgress progress = new HttpDownloadProgress(download);
					progress.OnDownloadUpdate += OnDownloadUpdateDelegate;
				}

				// queue it
				download.Run();

				started = true;
			}

			if (!started)
			{
				// handle gracefully and complete
				Complete();
			}
		}

		private void OnDownloadUpdateDelegate(object sender, EventArgs e)
		{
			// fire on main thread
			if (MessageHandler.HasMessageHandler())
				MessageQueue.Invoke(OnDownloadUpdate, sender, e);
		}

		private void CancelDownloads()
		{
			// cancel any existing downloads
			foreach (HttpDownload download in downloads)
			{
				if (download == null)
					continue;

				download.Cancel();
			}
		}

		private void Complete()
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(DownloadsCompleted, this, null);
			}
			else
			{
				if (DownloadsCompleted != null)
					DownloadsCompleted(this, null);
			}
		}

		private void Failed(HttpDownloadFailureEventArgs failureEventArgs)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(DownloadsFailed, this, failureEventArgs);
			}
			else
			{
				if (DownloadsFailed != null)
					DownloadsFailed(this, failureEventArgs);
			}
		}

		private void OnDownloadComplete(object sender, EventArgs e)
		{
			if (CancelCheck())
			{
				return;
			}

			HttpDownload completedDownload = sender as HttpDownload;

			// cache the download file paths
			downloadedFiles.Add(completedDownload.GetStoragePath());

			// check if all downloads are completed
			if (MarkComplete(completedDownload))
			{
				Complete();
			}
		}

		private void OnDownloadFailed(object sender, EventArgs e)
		{
			if (CancelCheck())
			{
				return;
			}

			// just fail straight away
			HttpDownload download = (HttpDownload)sender;

			// handle optional downloads
			if (download.IsOptional())
			{
				FrameworkLogger.Warning("Optional download failed: " + download.GetStoragePath());

				// check if all downloads are completed
				if (MarkComplete(download))
				{
					Complete();
				}

				return;
			}

			HttpDownloadFailureEventArgs failureEventArgs;

			// check if the file failed to save first and report that
			if (download.GetSaveError() != FileSaveResult.Success)
			{
				failureEventArgs = new HttpDownloadFailureEventArgs(download.GetSaveError());
			}
			else
			{
				failureEventArgs = new HttpDownloadFailureEventArgs(HttpDownloadFailure.DownloadFailed);
			}

			Failed(failureEventArgs);
		}

		private bool MarkComplete(HttpDownload completedDownload)
		{
			int downloadsLeft = 0;
			for (int i=0; i<downloads.Length; i++)
			{
				HttpDownload download = downloads[i];
				if (download == null)
					continue;

				if (download == completedDownload)
				{
					// remove this from the download list
					downloads[i] = null;
				}
				else
				{
					// keep track of how many downloads are still progressing
					downloadsLeft++;
				}
			}

			return downloadsLeft == 0;
		}
	}
}
