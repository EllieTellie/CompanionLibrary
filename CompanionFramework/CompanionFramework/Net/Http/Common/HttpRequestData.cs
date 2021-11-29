using CompanionFramework.Core.Threading.Messaging;
using System;
using System.Collections.Generic;

namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// Wrapper for all the parameters for a request over http.
	/// This allow for abstraction between Unity and .NET.
	/// </summary>
	public class HttpRequestData
	{
		private static readonly int DefaultNetworkTimeout = 10000;

		public readonly string uri;
		public readonly byte[] postData;
		public readonly string contentType;
		public Dictionary<string, string> headers; 
		public int timeout;

		/// <summary>
		/// Fired on the main thread when an <see cref="HttpResponse"/> is received for this request. Source is <see cref="HttpRequestData"/> and event arguments is <see cref="HttpEventArgs"/>
		/// </summary>
		public event EventHandler ResponseEvent;

		public HttpRequestData(string uri) : this (uri, null, DefaultNetworkTimeout, null)
		{
		}

		public HttpRequestData(string uri, byte[] postData) : this(uri, postData, DefaultNetworkTimeout, null)
		{
		}

		public HttpRequestData(string uri, byte[] postData, string contentType) : this(uri, postData, DefaultNetworkTimeout, contentType)
		{
		}

		public HttpRequestData(string uri, byte[] postData, int timeout) : this(uri, postData, timeout, null)
		{

		}

		public HttpRequestData(string uri, byte[] postData, int timeout, string contentType)
		{
			this.uri = uri;
			this.postData = postData;
			this.timeout = timeout;
			this.contentType = contentType;
		}

		public void AddHeader(string name, string value)
		{
			if (headers == null)
			{
				headers = new Dictionary<string, string>();
			}

			headers.Add(name, value);
		}

		public void HandleResponse(HttpResponse response)
		{
			MessageQueue.Invoke(ResponseEvent, this, new HttpEventArgs(response));
		}

		public Uri GetUri()
		{
			return new Uri(uri);
		}
	}
}
