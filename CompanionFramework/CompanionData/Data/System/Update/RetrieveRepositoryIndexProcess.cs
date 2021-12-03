using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using CompanionFramework.Net.Http.Common;
using System;

namespace Companion.Data.System.Update
{
	public class RetrieveRepositoryIndexProcess : CoreUpdateProcess
	{
		protected UpdateStateData state;

		protected readonly string url;
		protected readonly bool async;

		public RetrieveRepositoryIndexProcess(string url, bool async = true)
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


		public override UpdateState GetState()
		{
			return UpdateState.RetrieveRespositoryIndex;
		}
	}
}
