﻿using CompanionFramework.Core.Log;
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

		protected readonly string url;
		protected readonly bool async;

		public RetrieveGameSystemIndexProcess(string url, bool async = true)
		{
			this.url = url;
			this.async = async;
		}

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
