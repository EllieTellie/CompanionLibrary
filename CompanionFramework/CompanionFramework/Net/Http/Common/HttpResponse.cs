using System;
using System.Net;

namespace CompanionFramework.Net.Http.Common
{
	public enum NetworkResult
	{
		/// <summary>
		/// The http request was a success.
		/// </summary>
		Success,
		/// <summary>
		/// The http request resulted in error.
		/// </summary>
		Error,
		/// <summary>
		/// The http request was cancelled.
		/// </summary>
		Cancelled,
		/// <summary>
		/// The http request timed out.
		/// </summary>
		TimedOut
	}

	/// <summary>
	/// Stores the result of the http response. Holds the data that was received from the http stream.
	/// </summary>
	public class HttpResponse
	{
		public Exception Exception { get; set; }
		public string Message { get; set; }

		public NetworkResult Result { get; set; }
		public byte[] Data { get; set; }

		public HttpStatusCode ResponseCode { get; set; }

		public static readonly HttpResponse Error = new HttpResponse(NetworkResult.Error);
		public static readonly HttpResponse Cancelled = new HttpResponse(NetworkResult.Cancelled);

		public HttpResponse(NetworkResult result)
		{
			this.Result = result;
			this.Exception = null;
			this.Message = null;
			this.Data = null;
		}

		public HttpResponse(NetworkResult result, byte[] data)
		{
			this.Result = result;
			this.Exception = null;
			this.Message = null;
			this.Data = data;
		}

		public HttpResponse(NetworkResult result, byte[] data, HttpStatusCode responseCode)
		{
			this.Result = result;
			this.Exception = null;
			this.Message = null;
			this.Data = data;
			this.ResponseCode = responseCode;
		}

		public HttpResponse(NetworkResult result, Exception exception, string message)
		{
			this.Result = result;
			this.Exception = exception;
			this.Message = message;
			this.Data = null;
		}
	}
}
