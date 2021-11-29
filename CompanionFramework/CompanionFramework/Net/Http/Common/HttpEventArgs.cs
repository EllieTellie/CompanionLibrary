using CompanionFramework.IO.Utils;
using System;

namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// Wrapper class containing <see cref="HttpResponse"/>.
	/// </summary>
	public class HttpEventArgs : EventArgs
	{
		/// <summary>
		/// Http response from the request. Holds information about failure or success.
		/// </summary>
		public readonly HttpResponse response;

		public HttpEventArgs(HttpResponse response)
		{
			this.response = response;
		}

		/// <summary>
		/// Helper to get text from the response data. This will return null if the data is binary (non-string).
		/// </summary>
		/// <param name="e">EventArgs</param>
		/// <returns>Text from response or null if none present or not text</returns>
		public static string GetText(EventArgs e)
		{
			HttpEventArgs eventArgs = e as HttpEventArgs;
			if (eventArgs == null)
				return null;

			HttpResponse response = eventArgs.response;
			if (response == null)
			{
				//Logger.LogError("Error: response is null!");
				return null;
			}

			byte[] data = response.Data;
			if (data == null)
			{
				//Logger.LogError("Error: data is null!");
				return null;
			}

			return FileUtils.GetString(data);
		}
	}
}