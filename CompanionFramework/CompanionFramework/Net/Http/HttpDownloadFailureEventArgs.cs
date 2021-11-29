using CompanionFramework.IO.Utils;
using System;

namespace CompanionFramework.Net.Http
{
	/// <summary>
	/// Describes type of download failure
	/// </summary>
	public enum HttpDownloadFailure
	{
		Cancelled,
		DownloadFailed,
		SaveFailed
		//DecompressFailed // could be used if we decompress inside the download class
	}

	/// <summary>
	/// Stores download failure reason
	/// </summary>
	public class HttpDownloadFailureEventArgs : EventArgs
	{
		/// <summary>
		/// Type of download failure.
		/// </summary>
		public readonly HttpDownloadFailure failure;

		/// <summary>
		/// If the download failure was SaveFailed you can use this to determine the error.
		/// </summary>
		public readonly FileSaveResult saveResult;

		/// <summary>
		/// Create a new download failure class. For save failures use the other constructor.
		/// </summary>
		/// <param name="failure">Type of failure</param>
		public HttpDownloadFailureEventArgs(HttpDownloadFailure failure)
		{
			this.failure = failure;

			saveResult = FileSaveResult.Success;
		}

		/// <summary>
		/// Create a new download failure class. For net failures use the other constructor.
		/// </summary>
		/// <param name="result">File save result</param>
		public HttpDownloadFailureEventArgs(FileSaveResult result)
		{
			saveResult = result;

			failure = HttpDownloadFailure.SaveFailed;
		}
	}
}
