using CompanionFramework.Json.Extensions;
using LitJson;
using System;

namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// Stores a path in the json for easy retrieval.
	/// </summary>
	public class JsonPath
	{
		/// <summary>
		/// Constant define for ["data"]["result"]. Commonly used path in network results.
		/// </summary>
		public static readonly JsonPath ResultPath = new JsonPath("data", "result");

		private readonly string[] path;

		/// <summary>
		/// Create a path with the params matching the order of the params, for example: "data", "result" for the path: ["data"]["result"].
		/// </summary>
		/// <param name="path">Path as params</param>
		public JsonPath(params string[] path)
		{
			this.path = path;

			// validate path
			if (path == null)
				throw new ArgumentNullException("Path should not be null.");

			for (int i = 0; i < path.Length; i++)
			{
				string name = path[i];

				if (name == null || name.Length == 0)
					throw new ArgumentException("Invalid path");
			}
		}

		/// <summary>
		/// Whether this path exists in the json data.
		/// </summary>
		/// <param name="rootObject">Root object</param>
		/// <returns>True if this path exist in the json data</returns>
		public bool IsPath(JsonData rootObject)
		{
			JsonData jsonData = rootObject;
			for (int i = 0; i < path.Length; i++)
			{
				string name = path[i];

				jsonData = jsonData.Get(name);
				if (jsonData == null)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Get the json data at this path or null if this path was not found in the data.
		/// </summary>
		/// <param name="rootObject">Root object</param>
		/// <returns>JsonData for this path or null</returns>
		public JsonData Get(JsonData rootObject)
		{
			JsonData jsonData = rootObject;
			for (int i = 0; i < path.Length; i++)
			{
				string name = path[i];

				jsonData = jsonData.Get(name);
				if (jsonData == null)
					return null;
			}

			return jsonData;
		}
	}
}