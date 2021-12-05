using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Net.Http.Common;
using System;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Retrieve the data index from the repository index. This reads the .bsi file (battle scribe index/zipped xml) from the repository index url and create a <see cref="DataIndex"/> from it.
	/// </summary>
	public class RetrieveRepositoryIndexProcess : CoreUpdateProcess
	{
		protected UpdateStateData state;

		protected readonly string url;
		protected readonly bool async;

		/// <summary>
		/// Creates a process to retrieve the data index from the repository index.
		/// </summary>
		/// <param name="repository">Repository to get index url from</param>
		/// <param name="async">If true the networking will be asynchronous</param>
		public RetrieveRepositoryIndexProcess(Repository repository, bool async = true)
		{
			this.url = repository.indexUrl;
			this.async = async;
		}

		/// <summary>
		/// Creates a process to retrieve the data index from the repository index.
		/// </summary>
		/// <param name="url">Repository index url</param>
		/// <param name="async">If true the networking will be asynchronous</param>
		public RetrieveRepositoryIndexProcess(string url, bool async = true)
		{
			this.url = url;
			this.async = async;
		}

		/// <inheritdoc/>
		public override void Execute(UpdateStateData state)
		{
			this.state = state;

			RequestIndex(url);
		}

		private void RequestIndex(string url)
		{
			HttpRequestData requestData = new HttpRequestData(url);

			HttpRequest request = new HttpRequest(requestData);
			request.ResponseEvent += OnResponseEvent;
			request.Run(async);
		}

		private void OnResponseEvent(HttpResponse response)
		{
			if (response.Result != NetworkResult.Success || response.Data == null)
			{
				FrameworkLogger.Error("Unable to receive index");
				Abort();
				return;
			}

			DataIndex dataIndex = DataIndex.LoadDataIndexXml(response.Data);
			if (dataIndex == null)
			{
				FrameworkLogger.Error("Unable to parse data index");
				Abort();
				return;
			}

			// store it
			if (state != null)
				state.dataIndex = dataIndex;

			Complete(dataIndex);
		}

		/// <inheritdoc/>
		public override UpdateState GetState()
		{
			return UpdateState.RetrieveRespositoryIndex;
		}
	}
}
