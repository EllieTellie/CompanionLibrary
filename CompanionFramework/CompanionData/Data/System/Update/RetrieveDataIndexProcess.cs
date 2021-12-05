using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Net.Http.Common;
using System;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Retrieve the data index from the repository index. This reads the .bsi file (battle scribe index/zipped xml) from the repository index url and create a <see cref="DataIndex"/> from it.
	/// </summary>
	public class RetrieveDataIndexProcess : CoreUpdateProcess
	{
		protected RepositoryData state;

		protected readonly Repository repository;
		protected readonly string url;
		protected readonly bool async;

		/// <summary>
		/// Creates a process to retrieve the data index from the repository index.
		/// </summary>
		/// <param name="repository">Repository to get index url from</param>
		/// <param name="async">If true the networking will be asynchronous</param>
		public RetrieveDataIndexProcess(Repository repository, bool async = true)
		{
			this.repository = repository;
			this.url = repository.indexUrl;
			this.async = async;
		}

		/// <summary>
		/// Creates a process to retrieve the data index from the repository index.
		/// </summary>
		/// <param name="url">Repository index url</param>
		/// <param name="async">If true the networking will be asynchronous</param>
		public RetrieveDataIndexProcess(string url, bool async = true)
		{
			this.repository = null;
			this.url = url;
			this.async = async;
		}

		/// <inheritdoc/>
		public override void Execute(RepositoryData state)
		{
			this.state = state;

			if (state == null)
			{
				FrameworkLogger.Error("Missing repository data");
				Abort();
				return;
			}

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
			GameSystemData gameSystemData = state.AddGameSystem(repository, dataIndex);

			Complete(gameSystemData);
		}

		/// <inheritdoc/>
		public override UpdateState GetState()
		{
			return UpdateState.RetrieveRespositoryIndex;
		}
	}
}
