using CompanionFramework.IO.Utils;
using System;
using System.IO;
using System.Text;

namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// String builder that builds form body parameters for http requests.
	/// </summary>
	public class HttpFormParamBuilder : IDisposable
	{
		private MemoryStream memoryStream = new MemoryStream();

		private string formBoundary = null;

		private static string lineBreak = "\r\n";

		/// <summary>
		/// Create a new form builder.
		/// </summary>
		public HttpFormParamBuilder()
		{	
		}

		/// <summary>
		/// Add a parameter with a string value.
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddParam(string key, string value)
		{
			StringBuilder builder = new StringBuilder();

			if (memoryStream.Length != 0)
				builder.Append(lineBreak);

			// I think escape data string is correct here, but it might not be
			AddFormEntry(builder, key);
			builder.Append(lineBreak);
			builder.Append(lineBreak);
			builder.Append(value);

			Write(builder);
		}

		/// <summary>
		/// Add a file to the http body.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="data">File data</param>
		public void AddBinaryFile(string key, byte[] data)
		{
			AddBinaryFile(key, null, null, data);
		}

		/// <summary>
		/// Add a file to the body with optional filename and content type.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="fileName">File data</param>
		/// <param name="contentType">Content type</param>
		/// <param name="data">File data</param>
		public void AddBinaryFile(string key, string fileName, string contentType, byte[] data)
		{
			StringBuilder builder = new StringBuilder();

			if (memoryStream.Length != 0)
				builder.Append(lineBreak);

			AddFormEntry(builder, key);
			if (fileName != null)
			{
				WriteSeparatedValue(builder, "filename", fileName);
			}

			if (contentType != null)
			{
				builder.Append(lineBreak);
				builder.Append("Content-Type: ");
				builder.Append(contentType);
			}

			builder.Append(lineBreak);
			builder.Append(lineBreak);

			Write(builder);

			memoryStream.Write(data, 0, data.Length);
		}

		private void AddFormEntry(StringBuilder builder, string name)
		{
			builder.Append("--");
			builder.Append(GetFormBoundary());
			builder.Append(lineBreak);
			builder.Append("Content-Disposition: form-data");
			WriteSeparatedValue(builder, "name", name);
		}

		private void WriteSeparatedValue(StringBuilder builder, string key, string value)
		{
			builder.Append("; ");
			builder.Append(key);
			builder.Append("=\"");
			builder.Append(value);
			builder.Append("\"");
		}

		/// <summary>
		/// End the form by adding the form boundary. Should only be called once.
		/// </summary>
		public void EndForm()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(lineBreak);
			builder.Append("--");
			builder.Append(GetFormBoundary());
			builder.Append("--");
			builder.Append(lineBreak);

			Write(builder);
		}

		private void Write(StringBuilder builder)
		{
			string text = builder.ToString();

			byte[] bytes = FileUtils.GetBytes(text);
			memoryStream.Write(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Get all the data. Call <see cref="EndForm"/> before calling this method.
		/// </summary>
		/// <returns>Byte data</returns>
		public byte[] GetData()
		{
			return memoryStream.ToArray();
		}

		/// <summary>
		/// Get the form content type for the http request including the boundary.
		/// </summary>
		/// <returns>Content type for the http request</returns>
		public string GetContentType()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("multipart/form-data; boundary=");
			builder.Append(GetFormBoundary());

			return builder.ToString();
		}

		private string GetFormBoundary()
		{
			if (formBoundary == null)
			{
				formBoundary = string.Format("----{0:N}", Guid.NewGuid());
			}

			return formBoundary;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					memoryStream.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
	}
}
