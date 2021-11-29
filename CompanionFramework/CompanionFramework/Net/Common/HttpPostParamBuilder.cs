using CompanionFramework.Core.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// String builder that builds post parameters and escapes string data for sending over http.
	/// </summary>
	public class HttpPostParamBuilder
	{
		private readonly StringBuilder builder;

		/// <summary>
		/// Create a new builder which creates an empty string builder.
		/// </summary>
		public HttpPostParamBuilder()
		{
			builder = new StringBuilder();
		}

		/// <summary>
		/// Create a new builder with the text provided. The text does not get validated.
		/// </summary>
		/// <param name="text">initial text</param>
		public HttpPostParamBuilder(string text)
		{
			this.builder = new StringBuilder(text);
		}

		/// <summary>
		/// Add multiple parameters. These parameters should already be escaped.
		/// </summary>
		/// <param name="formattedParams">string to append</param>
		public void AddParams(string formattedParams)
		{
			if (formattedParams == null)
				return;

			if (builder.Length != 0)
				builder.Append('&');

			builder.Append(formattedParams);
		}

		/// <summary>
		/// Add a parameter with a long value.
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddParam(string key, long value)
		{
			AddParam(key, value.ToString());
		}

		/// <summary>
		/// Add a parameter with an integer value.
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddParam(string key, int value)
		{
			AddParam(key, value.ToString());
		}

		/// <summary>
		/// Add a parameter with a string value.
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddParam(string key, string value)
		{
			if (value == null)
			{
				FrameworkLogger.Warning("Param: " + key + " does not have a value set, ignoring.");
				return;
			}

			if (builder.Length != 0)
				builder.Append('&');

			// I think escape data string is correct here, but it might not be
			builder.Append(Uri.EscapeDataString(key));
			builder.Append('=');
			builder.Append(Uri.EscapeDataString(value));
		}

		/// <summary>
		/// Send array of values as a command.
		/// </summary>
		/// <typeparam name="T">The object to send strings normally</typeparam>
		/// <param name="key">The key for example fields[]</param>
		/// <param name="values">The array values to send</param>
		public void AddArrayParam<T>(string key, IList<T> values)
		{
			for(int i = 0; i < values.Count; i++)
			{
				AddParam(key, values[i].ToString());
			}
		}

		public override string ToString()
		{
			return builder.ToString();
		}
	}
}
