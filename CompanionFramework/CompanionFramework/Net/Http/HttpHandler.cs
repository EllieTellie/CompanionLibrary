using System;
using CompanionFramework.Core.Threading.ThreadPool;
using CompanionFramework.Net.Http.Common;

namespace CompanionFramework.Net.Http
{
	/// <summary>
	/// Handles System.Net http requests on multiple threads using <see cref="SimpleThreadPool"/>. The user can specify the number of threads it should use.
	/// </summary>
	public class HttpHandler : IHttpHandler
	{
		private SimpleThreadPool threadPool;
		private bool disposed;

		/// <summary>
		/// Creates the http handler with one thread.
		/// </summary>
		public HttpHandler() : this(1)
		{
		}

		/// <summary>
		/// Creates the http handler with the specified number of threads.
		/// </summary>
		/// <param name="threads">Number of threads</param>
		public HttpHandler(int threads)
		{
			threadPool = new SimpleThreadPool(threads);
		}

		/// <summary>
		/// Add the http request to the thread pool to run it.
		/// </summary>
		/// <param name="requestData">Request data</param>
		public void QueueHttpRequest(HttpRequestData requestData)
		{
			HttpRequest request = new HttpRequest(requestData);
			requestData.timeout = 60000; // 1 minute
			threadPool.Add(request);
		}

		/// <summary>
		/// Add the http request to the thread pool to run it.
		/// </summary>
		/// <param name="requestData">Request data</param>
		/// <param name="progress">Progress tracker</param>
		public void QueueHttpRequest(HttpRequestData requestData, IStreamProgress progress)
		{
			HttpRequest request = new HttpRequest(requestData, progress);
			requestData.timeout = 60000; // 1 minute
			threadPool.Add(request);
		}

		/// <summary>
		/// How many requests are currently queued.
		/// </summary>
		/// <returns></returns>
		public int QueuedRequestCount()
		{
			return threadPool.Count();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				threadPool.Dispose();
			}

			disposed = true;
		}

		~HttpHandler()
		{
			Dispose(false);
		}
	}
}
