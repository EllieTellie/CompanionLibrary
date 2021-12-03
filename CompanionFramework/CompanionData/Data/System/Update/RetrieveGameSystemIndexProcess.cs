using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Json.Utils;
using CompanionFramework.Net.Http.Common;
using LitJson;
using System;

namespace Companion.Data.System.Update
{
	public class RetrieveGameSystemIndexProcess : CoreUpdateProcess
	{
		protected UpdateStateData state;

		public override void Execute(UpdateStateData state)
		{
			this.state = state;

			string hardcodedUrl = "https://battlescribedata.appspot.com/repos"; // move to file
			RequestIndex(hardcodedUrl);
		}

		private void RequestIndex(string hardcodedUrl)
		{
			HttpRequestData requestData = new HttpRequestData(hardcodedUrl);

			HttpRequest request = new HttpRequest(requestData);
			request.ResponseEvent += OnResponseEvent;
			request.Run(false);
		}

		private void OnResponseEvent(HttpResponse response)
		{
			if (response.Result != NetworkResult.Success || response.Data == null)
			{
				FrameworkLogger.Error("Unable to receive index");
				Abort();
				return;
			}

			string text = FileUtils.GetString(response.Data);

			JsonData jsonData = JsonUtils.ConvertToJsonData(text);
			if (jsonData == null || !jsonData.IsObject)
			{
				FrameworkLogger.Error("Index is not valid json most likely");
				Abort();
				return;
			}

			RepositoryIndex repositoryIndex = RepositoryIndex.Parse(jsonData);
			if (repositoryIndex == null)
			{
				FrameworkLogger.Error("Unable to parse repo");
				Abort();
				return;
			}

			if (state != null)
				state.repositoryIndex = repositoryIndex;

			Complete(repositoryIndex);
		}

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
