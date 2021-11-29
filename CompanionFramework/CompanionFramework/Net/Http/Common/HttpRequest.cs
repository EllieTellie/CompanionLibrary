using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using CompanionFramework.Core.Threading.ThreadPool;
using CompanionFramework.IO.Utils;

namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// Http request that uses System.Net.
	/// </summary>
	public class HttpRequest : IBaseThreadedTask
	{
		//private static bool TriggeredTimeout = false;

		public delegate void HttpResponseEvent(HttpResponse response);

		private bool cancelled = false;

		/// <summary>
		/// Fired as a delegate when a response is received.
		/// </summary>
		public HttpResponseEvent ResponseEvent;

		private readonly HttpRequestData requestData;
		private readonly IStreamProgress progress;

		#region constructors and destructors
		public HttpRequest(HttpRequestData requestData) : this(requestData, null)
		{
		}

		public HttpRequest(HttpRequestData requestData, IStreamProgress progress)
		{
			this.requestData = requestData;
			this.progress = progress;

			// pass through the response event
			ResponseEvent += requestData.HandleResponse;
		}
		#endregion

		/// <summary>
		/// Execute this http request synchronously.
		/// </summary>
		public void Run()
		{
			Run(false);
		}

		/// <summary>
		/// Execute this http request synchronously or asynchronously.
		/// </summary>
		public void Run(bool async)
		{
			// check if cancelled
			if (cancelled)
			{
				Complete(HttpResponse.Cancelled);
				return;
			}

			// create the request
			HttpWebRequest request = SetupRequest();

			if (async)
			{
				RequestAsync(request);
			}
			else
			{
				RequestSync(request);
			}
		}

		private void RequestSync(HttpWebRequest request)
		{
			try
			{
				//NetLogger.Message("requesting: " + Uri);

				// add headers
				AddHeaders(requestData, request.Headers);

				if (requestData.postData != null && request.Method.Equals("POST"))
				{
					// set content length
					request.ContentLength = requestData.postData.Length;

					// get ready to post data
					//NetLogger.Message("---- Get Stream ----");
					using (Stream requestStream = request.GetRequestStream())
					{
						requestStream.WriteTimeout = requestData.timeout;
						//NetLogger.Message("Write timeout = " + requestStream.WriteTimeout + " Can write: " + requestStream.CanWrite);

						//NetLogger.Message("---- Pre Write Stream ----");
						requestStream.Write(requestData.postData, 0, requestData.postData.Length);
						//NetLogger.Message("---- Post Write Stream ----");
						requestStream.Flush();
						//NetLogger.Message("---- Post Flush ----");
					}

					//NetLogger.Message("sent post request");
				}

				//NetLogger.Message("asking for response");

				// send to server
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					//NetLogger.Message("received response: " + response.StatusCode + " Description: " + response.StatusDescription);

					// print out headers
					//WebHeaderCollection headers = response.Headers;
					//for (int i = 0; i < response.Headers.Count; i++)
					//{
					//	string key = response.Headers.GetKey(i);
					//	string header = response.Headers[i];
					//	NetLogger.Message(key + ": " + header);
					//}

					HttpStatusCode statusCode = response.StatusCode;
					if (statusCode != HttpStatusCode.OK)
					{
						// attempt to read a response from http failures
						using (Stream responseStream = response.GetResponseStream())
						{
							byte[] data = FileUtils.GetByteArrayFromStream(responseStream, progress, response.ContentLength);

							// fire event
							if (data != null)
							{
								Complete(new HttpResponse(NetworkResult.Error, data, statusCode));
								return;
							}
						}

						if (statusCode == HttpStatusCode.PartialContent)
							NetLogger.Warning("Failed request: received partial content");

						NetLogger.Message("Received Http Status Code: " + statusCode);

						Complete(new HttpResponse(NetworkResult.Error, null, statusCode));
					}
					else
					{
						// get byte data
						using (Stream responseStream = response.GetResponseStream())
						{
							byte[] data = FileUtils.GetByteArrayFromStream(responseStream, progress, response.ContentLength);

							// fire event
							Complete(new HttpResponse(NetworkResult.Success, data, statusCode));
						}
					}
				}
			}
			catch (WebException e)
			{
				HttpWebResponse response = e.Response as HttpWebResponse;
				if (response != null && response.ContentLength > 0)
				{
					// get byte data
					using (Stream responseStream = response.GetResponseStream())
					{
						byte[] data = FileUtils.GetByteArrayFromStream(responseStream, progress, response.ContentLength);

						// fire event
						Complete(new HttpResponse(NetworkResult.Error, data, response.StatusCode));
						return;
					}
				}

				NetLogger.Exception(e);
				Complete(new HttpResponse(NetworkResult.Error, e, e.Message));
			}
			catch (Exception e)
			{
				// if a timeout happened flip the flag
				//if (e is TimeoutException)
				//{
				//	TriggeredTimeout = true;
				//}

				NetLogger.Exception(e);
				Complete(new HttpResponse(NetworkResult.Error, e, e.Message));
			}
		}

		private void AddHeaders(HttpRequestData requestData, WebHeaderCollection headers)
		{
			if (requestData.headers != null)
			{
				foreach (KeyValuePair<string, string> entry in requestData.headers)
				{
					headers.Add(entry.Key, entry.Value);
				}
			}
		}

		private void RequestAsync(HttpWebRequest request)
		{
			try
			{
				if (requestData.postData != null && request.Method.Equals("POST"))
				{
					// get ready to post data
					request.BeginGetRequestStream(new AsyncCallback(WritePostData), request);
					//NetLogger.Message("post request");
				}
				else
				{
					// send to server
					request.BeginGetResponse(new AsyncCallback(ResponseReceived), request);
					//NetLogger.Message("get request");
				}
			}
			catch (Exception e)
			{
				NetLogger.Exception(e);
				Complete(HttpResponse.Error);
			}
		}

		private void WritePostData(IAsyncResult result)
		{
			// check if cancelled
			if (cancelled)
			{
				Complete(HttpResponse.Cancelled);
				return;
			}

			HttpWebRequest request = (HttpWebRequest)result.AsyncState;

			try
			{
				Stream stream = request.EndGetRequestStream(result);

				//NetLogger.Message("end request stream");

				// write post params
				using (stream)
				{
					stream.Write(requestData.postData, 0, requestData.postData.Length);
				}

				//NetLogger.Message("post data sent");

				// send to server
				request.BeginGetResponse(new AsyncCallback(ResponseReceived), request);

				//NetLogger.Message("wait for response");
			}
			catch (Exception e)
			{
				NetLogger.Exception(e);
				Complete(HttpResponse.Error);
			}
		}

		private void ResponseReceived(IAsyncResult result)
		{
			// check if cancelled
			if (cancelled)
			{
				Complete(HttpResponse.Cancelled);
				return;
			}

			//NetLogger.Message("response received.");

			HttpWebRequest request = (HttpWebRequest)result.AsyncState;

			try
			{
				HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

				NetLogger.Message("Http Code: " + response.StatusCode + " Description: " + response.StatusDescription);

				// get byte data
				using (Stream responseStream = response.GetResponseStream())
				{
					byte[] data = FileUtils.GetByteArrayFromStream(responseStream, progress, response.ContentLength);

					// fire event
					Complete(new HttpResponse(NetworkResult.Success, data));
				}
			}
			catch (WebException e)
			{
				HttpWebResponse response = e.Response as HttpWebResponse;
				if (response != null && response.ContentLength > 0)
				{
					// get byte data
					using (Stream responseStream = response.GetResponseStream())
					{
						byte[] data = FileUtils.GetByteArrayFromStream(responseStream, progress, response.ContentLength);

						// fire event
						Complete(new HttpResponse(NetworkResult.Error, data, response.StatusCode));
						return;
					}
				}

				NetLogger.Exception(e);
				Complete(new HttpResponse(NetworkResult.Error, e, e.Message));
			}
			catch (Exception e)
			{
				NetLogger.Exception(e);
				Complete(HttpResponse.Error);
			}
		}

		/// <summary>
		/// Attempt to cancel the http request.
		/// </summary>
		public void Cancel()
		{
			cancelled = true;
		}

		private HttpWebRequest SetupRequest()
		{
			HttpWebRequest request = null;
			try
			{
				request = (HttpWebRequest)WebRequest.Create(requestData.GetUri());
				request.Timeout = requestData.timeout;
				request.ReadWriteTimeout = requestData.timeout;
				request.KeepAlive = false; // apache server does not support it and sends "Connection: Close" after every request
			}
			catch (Exception e)
			{
				NetLogger.Exception(e);
				return null;
			}

			if (requestData.postData != null)
			{
				request.Method = "POST";
				request.ContentType = requestData.contentType != null ? requestData.contentType : "application/x-www-form-urlencoded";
			}

			return request;
		}

		private void Complete(HttpResponse response)
		{
			try
			{
				if (ResponseEvent != null)
					ResponseEvent(response);
			}
			catch (Exception e)
			{
				NetLogger.Exception(e);
			}

			//MessageQueue.Invoke(ResponseEvent, this, new HttpEventArgs(response));
		}

		/// <summary>
		/// Get the request data belong to this request.
		/// </summary>
		/// <returns>Request data</returns>
		public HttpRequestData GetRequestData()
		{
			return requestData;
		}
	}
}
