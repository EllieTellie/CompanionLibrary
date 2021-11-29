using CompanionFramework.Net.Http.Common;
using System;

namespace CompanionFramework.Net.Http
{
	/// <summary>
	/// Interface to allow http requests to be handled differently.
	/// </summary>
	public interface IHttpHandler : IDisposable
	{
		/// <summary>
		/// Queue this http request. The request may not execute immediately.
		/// </summary>
		/// <param name="requestData">Request Data for this request</param>
		void QueueHttpRequest(HttpRequestData requestData);

		/// <summary>
		/// Queue this http request. The request may not execute immediately.
		/// </summary>
		/// <param name="requestData">Request Data for this request</param>
		/// <param name="progress">Download progress tracker or null if not required</param>
		void QueueHttpRequest(HttpRequestData requestData, IStreamProgress progress);
	}
}
