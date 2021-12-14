using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Json.Utils;
using CompanionFramework.Net.Http.Common;
using LitJson;
using System;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Retrieves the index of game systems from the repository url. This reads the json from the repository and creates a <see cref="RepositoryIndex"/> from it.
	/// </summary>
	public class RetrieveRepositoryIndexProcess : CoreUpdateProcess
	{
		protected RepositoryData state;

		protected readonly string url;
		protected readonly bool async;

		/// <summary>
		/// Creates a process to retrieve the index of game systems.
		/// </summary>
		/// <param name="url">Repository url</param>
		/// <param name="async">If true the networking will be asynchronous</param>
		public RetrieveRepositoryIndexProcess(string url, bool async = true)
		{
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

			// update url
			state.url = url;

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

			string text = FileUtils.GetString(response.Data);

			JsonData jsonData = JsonUtils.ConvertToJsonData(text);
			if (jsonData == null || !jsonData.IsObject)
			{
				Abort(UpdateError.InvalidRepositoryIndex, "Index is not valid json");
				return;
			}

			RepositoryIndex repositoryIndex = RepositoryIndex.Parse(jsonData);
			if (repositoryIndex == null)
			{
				Abort(UpdateError.InvalidRepositoryIndex, "Unable to parse repo");
				return;
			}

			state.repositoryIndex = repositoryIndex;

			Complete(repositoryIndex);
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
			FireCompleteState(state);

			base.Complete(result);
		}

		private void FireFailedState(UpdateError error, string message)
		{
			ProcessFailedEventArgs eventArgs = new ProcessFailedEventArgs(state, error, message);
			state.FireDataIndexFailed(eventArgs);
		}

		private void FireCompleteState(RepositoryData repositoryData)
		{
			RepositoryIndexSuccessEventArgs eventArgs = new RepositoryIndexSuccessEventArgs(repositoryData);
			state.FireDataIndexAdded(eventArgs);
		}

		/// <inheritdoc/>
		public override UpdateState GetState()
		{
			return UpdateState.RetrieveGameSystemIndex;
		}

		protected override void Cleanup()
		{
			base.Cleanup();

			// clear state
			state = null;
		}
	}
}
