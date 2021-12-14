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
				Abort(UpdateError.MissingState, "Missing repository data");
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
				Abort(UpdateError.FailedNetworkResponse, "Unable to receive index");
				return;
			}

			DataIndex dataIndex = DataIndex.LoadDataIndexXml(response.Data);
			if (dataIndex == null)
			{
				Abort(UpdateError.InvalidDataIndex, "Unable to parse data index");
				return;
			}

			// store it
			GameSystemData gameSystemData = state.AddGameSystem(repository, dataIndex);

			// fallback for if this is called with a url only
			if (gameSystemData == null)
				gameSystemData = new GameSystemData(null, dataIndex);

			Complete(gameSystemData);
		}

		public override void Abort(UpdateError error, string message = null)
		{
			// fire event on the RepositoryData as a passthrough
			FireFailedState(error, message);

			base.Abort(error, message);
		}

		protected override void Complete(object result)
		{
			// fire event on the RepositoryData as a passthrough
			FireCompleteState((GameSystemData)result);

			base.Complete(result);
		}

		private void FireFailedState(UpdateError error, string message)
		{
			ProcessFailedEventArgs eventArgs = new ProcessFailedEventArgs(state, error, message);
			state.FireDataIndexFailed(eventArgs);
		}

		private void FireCompleteState(GameSystemData gameSystemData)
		{
			DataIndexSuccessEventArgs eventArgs = new DataIndexSuccessEventArgs(state, gameSystemData.repository, gameSystemData.dataIndex);
			state.FireDataIndexAdded(eventArgs);
		}

		/// <inheritdoc/>
		public override UpdateState GetState()
		{
			return UpdateState.RetrieveRespositoryIndex;
		}
	}
}
