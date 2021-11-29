using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;
using LitJson;
using System;
using System.IO;

namespace CompanionFramework.Json.Utils
{
	public static class JsonUtils
	{
		/// <summary>
		/// Convert file to compact json. This reads the file and overwrites it with the compact json.
		/// NOTE: There is no guarantee that the order of the file is preserved.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <returns>True if compacted and saved</returns>
		public static bool ConvertToCompactJson(string filePath)
		{
			string text = FileUtils.ReadTextFileSimple(filePath);

			if (text == null)
				return false;

			JsonData jsonData = ConvertToJsonData(text);
			if (jsonData == null)
				return false;

			string convertedJson = JsonMapper.ToJson(jsonData);
			return FileUtils.Save(FileUtils.GetBytes(convertedJson), filePath) == FileSaveResult.Success;
		}

		/// <summary>
		/// Attempt to parse the text to <see cref="JsonData"/>. If it does not succeed it returns null.
		/// </summary>
		/// <param name="text">text to parse</param>
		/// <returns>Json data or null if failed to parse</returns>
		public static JsonData ConvertToJsonData(string text)
		{
			if (text == null)
				return null;

			try
			{
				return JsonMapper.ToObject(text);
			}
			catch (JsonException e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}

		/// <summary>
		/// Attempt to parse the text to <see cref="JsonData"/>. If it does not succeed it returns null and attempts to log the text to the error logs.
		/// </summary>
		/// <param name="text">text to parse</param>
		/// <returns>Json data or null if failed to parse</returns>
		public static JsonData ConvertToJsonDataAndLogText(string text)
		{
			if (text == null)
				return null;

			try
			{
				return JsonMapper.ToObject(text);
			}
			catch (JsonException e)
			{
				FrameworkLogger.Exception(e);
				FrameworkLogger.Error("Failed to read json: " + text);
				return null;
			}
		}

		/// <summary>
		/// Attempt to parse the text from <see cref="TextReader"/> to <see cref="JsonData"/>. If it does not succeed return null.
		/// </summary>
		/// <param name="textReader">text to parse</param>
		/// <returns>Json data or null if failed to parse</returns>
		public static JsonData ConvertToJsonDataReader(TextReader textReader)
		{
			if (textReader == null)
				return null;

			try
			{
				return JsonMapper.ToObject(textReader);
			}
			catch (JsonException e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}

		/// <summary>
		/// Attempt to parse the text to <see cref="JsonData"/>. If it does not succeed it returns null.
		/// </summary>
		/// <param name="text">text to parse</param>
		/// <returns>Json data or null if failed to parse</returns>
		public static JsonData ConvertToObject(string text)
		{
			if (text == null)
				return null;

			try
			{
				return JsonMapper.ToObject(text);
			}
			catch (JsonException e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}

		/// <summary>
		/// Convert to a serialised object.
		/// </summary>
		/// <typeparam name="T">Type of serialised object</typeparam>
		/// <param name="text">Text to parse</param>
		/// <returns>Serialised object or null if failed to parse</returns>
		public static T ConvertToObject<T>(string text)
		{
			if (text == null)
				return default(T);

			try
			{
				return JsonMapper.ToObject<T>(text);
			}
			catch (JsonException e)
			{
				FrameworkLogger.Exception(e);
				return default(T);
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return default(T);
			}
		}
	}
}