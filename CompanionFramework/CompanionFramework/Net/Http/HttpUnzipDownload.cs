using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Net.Http.Common;
using System;
using System.IO;

namespace CompanionFramework.Net.Http
{
	/// <summary>
	/// Unzips a gzip compressed file received from a url to a file.
	/// </summary>
	public class HttpUnzipDownload
	{
		public readonly HttpRequestData requestData;
		public readonly string storagePath;

		protected readonly string fileName;
		protected readonly string outputExtension;
		protected readonly bool saveZippedData;

		private HttpRequest request;

		/// <summary>
		/// Fired when completed.
		/// </summary>
		public event Action CompleteEvent;

		/// <summary>
		/// Fired when failed.
		/// </summary>
		public event Action FailedEvent;

		/// <summary>
		/// Download class to download a file and unzip it immediately.
		/// </summary>
		/// <param name="requestData">Request data</param>
		/// <param name="storagePath">Storage path</param>
		/// <param name="outputExtension">Output extension</param>
		/// <param name="saveZippedData">True if we want to also save the zipped data</param>
		public HttpUnzipDownload(HttpRequestData requestData, string storagePath, string outputExtension = ".json", bool saveZippedData = true)
		{
			this.requestData = requestData;
			this.storagePath = storagePath;
			this.outputExtension = outputExtension;
			this.saveZippedData = saveZippedData;

			fileName = FileUtils.GetFileNameWithoutExtensionSafe(requestData.uri);
			if (string.IsNullOrEmpty(fileName))
				fileName = "invalid";
		}

		/// <summary>
		/// Fires the failed event.
		/// </summary>
		protected void Abort()
		{
			if (FailedEvent != null)
				FailedEvent();
		}

		/// <summary>
		/// Fires the complete event.
		/// </summary>
		protected void Complete()
		{
			if (CompleteEvent != null)
				CompleteEvent();
		}

		/// <summary>
		/// Starts the download.
		/// </summary>
		public void Download()
		{
			if (request != null)
			{
				FrameworkLogger.Warning("Already downloading");
				return;
			}

			request = new HttpRequest(requestData);
			request.ResponseEvent += OnDownloadResponse;
			request.Run(true);
		}

		private void OnDownloadResponse(HttpResponse httpResponse)
		{
			if (httpResponse.Result != NetworkResult.Success)
			{
				Abort();
				return;
			}

			// get extension from the downloaded file
			string extension = FileUtils.GetExtensionSafe(requestData.uri);
			if (!extension.Equals(".gz") && !extension.Equals(".gzip"))
				FrameworkLogger.Warning("File extension was not gzip");

			// unzip
			if (CompressionUtils.DecompressGZipToPath(httpResponse.Data, Path.Combine(storagePath, fileName + outputExtension)) != FileSaveResult.Success)
			{
				Abort();
				return;
			}

			if (saveZippedData)
			{
				// save original file
				if (FileUtils.Save(httpResponse.Data, Path.Combine(storagePath, fileName + extension)) != FileSaveResult.Success)
				{
					Abort();
					return;
				}
			}

			// mark download as complete
			Complete();
		}
	}
}
