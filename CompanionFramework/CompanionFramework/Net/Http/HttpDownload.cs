using System;
using CompanionFramework.Net.Http.Common;
using CompanionFramework.Core.Threading.ThreadPool;
using CompanionFramework.IO.Utils;
using CompanionFramework.Core.Threading.Messaging;

namespace CompanionFramework.Net.Http
{
	/// <summary>
	/// Download class to download files over http and saves them to disk.
	/// </summary>
	public class HttpDownload : IThreadedTask
	{
		/// <summary>
		/// The http handler to run the request through.
		/// </summary>
		private readonly IHttpHandler httpHandler;

		/// <summary>
		/// The request data for the download.
		/// </summary>
		private readonly HttpRequestData requestData;

		/// <summary>
		/// The path this file will be stored at.
		/// </summary>
		private readonly string storagePath;

		/// <summary>
		/// How many times to retry.
		/// </summary>
		private int retries;

		/// <summary>
		/// How many times we have attempted the download.
		/// </summary>
		private int numAttempts;

		/// <summary>
		/// Cancel token to cancel thread safe.
		/// </summary>
		private CancelToken cancelToken = new CancelToken();

		/// <summary>
		/// Optional stream downloading progress reporter.
		/// </summary>
		private IStreamProgress progress = null;

		/// <summary>
		/// Fired on the event system when the download failed. Source is <see cref="HttpDownload"/>. EventArgs is <see cref="HttpEventArgs"/>.
		/// </summary>
		public EventHandler DownloadFailed;

		/// <summary>
		/// Fired on the event system when the download completed. Source is <see cref="HttpDownload"/>.  EventArgs is <see cref="HttpEventArgs"/>.
		/// </summary>
		public EventHandler DownloadCompleted;

		/// <summary>
		/// Fired immediately every time we attempt the download. Can be called multiple times when retrying.
		/// </summary>
		public event TaskEvent TaskStarted;

		/// <summary>
		/// Fired immediately when the download completed or failed. Only fired once.
		/// </summary>
		public event TaskEvent TaskFinished;

		// cache save result, use GetSaveError() to access
		private FileSaveResult saveResult = FileSaveResult.Success;

		/// <summary>
		/// Whether this download is optional and the game can continue without. By default it's never optional. See: <see cref="HttpDownloadSet"/>.
		/// </summary>
		private bool optional = false;

		private bool async = true;

		public HttpDownload(IHttpHandler httpHandler, HttpRequestData requestData, string storagePath) : this(httpHandler, requestData, storagePath, 2)
		{
		}

		public HttpDownload(IHttpHandler httpHandler, HttpRequestData requestData, string storagePath, int retries)
		{
			this.httpHandler = httpHandler;
			this.retries = retries;
			this.requestData = requestData;
			this.storagePath = storagePath;

			Init();
		}

		public HttpRequestData GetRequestData()
		{
			return requestData;
		}

		private void Init()
		{
			// listen to request
			requestData.ResponseEvent += OnDownloadResponse;
		}

		/// <summary>
		/// Start the download.
		/// </summary>
		public void Run()
		{
			// check if we are cancelled
			CancelCheck();

			if (TaskStarted != null)
				TaskStarted(this);

			if (httpHandler != null)
			{
				httpHandler.QueueHttpRequest(requestData, progress);
			}
			else
			{
				HttpRequest request = new HttpRequest(requestData, progress);
				request.Run(async);
			}
		}

		private void OnDownloadResponse(object sender, EventArgs e)
		{
			// check if we are cancelled
			CancelCheck();

			// validate
			HttpEventArgs eventArgs = e as HttpEventArgs;
			if (!ValidateResponseEvent(eventArgs))
			{
				AttemptRetry(eventArgs);
				return;
			}

			if (storagePath != null)
			{
				// save to disk
				saveResult = FileUtils.Save(eventArgs.response.Data, storagePath);
				if (saveResult == FileSaveResult.Success)
				{
					Complete(eventArgs);
				}
				else
				{
					// breaking change: don't attempt to retry as we are likely to fail again
					Failed(eventArgs);

					//AttemptRetry(eventArgs);
				}
			}
			else
			{
				// if storage path is null we don't want to save it
				Complete(eventArgs);
			}
		}

		private void AttemptRetry(HttpEventArgs eventArgs)
		{
			numAttempts++;
			if (numAttempts < retries)
			{
				Run();
			}
			else
			{
				Failed(eventArgs);
			}
		}

		private bool ValidateResponseEvent(HttpEventArgs eventArgs)
		{
			if (eventArgs == null || eventArgs.response == null || eventArgs.response.Data == null || eventArgs.response.Result != NetworkResult.Success)
				return false;

			return true;
		}

		private void CancelCheck()
		{
			if (cancelToken.IsCancelRequest)
			{
				Failed();

				throw new OperationCanceledException();
			}
		}

		/// <summary>
		/// Attempt to cancel the download.
		/// </summary>
		public void Cancel()
		{
			cancelToken.Cancel();
		}

		/// <summary>
		/// Called when this download is completed and fires DownloadCompleted and TaskFinished.
		/// </summary>
		public void Complete(HttpEventArgs eventArgs = null)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(DownloadCompleted, this, eventArgs);
			}
			else
			{
				if (DownloadCompleted != null)
					DownloadCompleted(this, eventArgs);
			}	

			if (TaskFinished != null)
				TaskFinished(this);
		}

		/// <summary>
		/// Called when this download failed and fires DownloadFailed and TaskFinished.
		/// </summary>
		public void Failed(HttpEventArgs eventArgs = null)
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(DownloadFailed, this, eventArgs);
			}
			else
			{
				if (DownloadFailed != null)
					DownloadFailed(this, eventArgs);
			}

			if (TaskFinished != null)
				TaskFinished(this);
		}

		/// <summary>
		/// Return the storage path where this file will be saved.
		/// </summary>
		/// <returns>Storage path</returns>
		public string GetStoragePath()
		{
			return storagePath;
		}

		/// <summary>
		/// Set the download progress tracker.
		/// </summary>
		/// <param name="progress">Stream progress</param>
		public void SetProgressTracker(IStreamProgress progress)
		{
			this.progress = progress;
		}

		/// <summary>
		/// Save result is success by default, if an error occurred the save result will not be success.
		/// </summary>
		/// <returns>File save result</returns>
		public FileSaveResult GetSaveError()
		{
			return saveResult;
		}

		/// <summary>
		/// Whether this download is optional and can fail silently if it's missing.
		/// </summary>
		/// <returns>True if optional</returns>
		public bool IsOptional()
		{
			return optional;
		}

		/// <summary>
		/// Make this download optional.
		/// </summary>
		public void SetOptional()
		{
			optional = true;
			retries = 1; // don't allow it to retry the download if it's optional
		}

		/// <summary>
		/// This is for debugging mostly but it allows the download to run not async.
		/// </summary>
		/// <param name="async">Async</param>
		public void SetAsync(bool async)
		{
			this.async = async;
		}
	}
}
