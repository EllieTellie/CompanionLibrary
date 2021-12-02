using CompanionFramework.Core.Log;
using LitJson;
using System;

namespace CompanionFramework.Json.Extensions
{
	/// <summary>
	/// Extensions for LitJson JsonData objects.
	/// </summary>
	public static class CommonJsonDataExtensions
	{
		/// <summary>
		/// Get the int value of this json. If it's not an int or null it will return -1.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <returns>Int value or -1 if not an int</returns>
		public static int GetInt(this JsonData jsonData)
		{
			return GetInt(jsonData, true);
		}

		/// <summary>
		/// Get the int value of this json. If it's not an int or null it will return -1.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="key">key to look for</param>
		/// <returns>Int value or -1 if not an int</returns>
		public static int GetInt(this JsonData jsonData, string key)
		{
			return Get(jsonData, key).GetInt(true);
		}

		/// <summary>
		/// Get the int value of this json. If it's not an int or null it will return -1.
		/// Will log a warning if the data is not an int if the warn flag is set.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="warn">Whether to warn if the data is not an int</param>
		/// <returns>Int value or -1 if not an int</returns>
		public static int GetInt(this JsonData jsonData, bool warn)
		{
			if (jsonData == null || !jsonData.IsInt)
			{
				if (warn)
					FrameworkLogger.Warning("Data is not an integer");
				return -1;
			}

			return (int)jsonData;
		}

		/// <summary>
		/// Get the float value of this json. If it's not a float or null it will return -1f.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <returns>Float value or -1f if not a float</returns>
		[Obsolete("Use GetDouble() instead")]
		public static float GetFloat(this JsonData jsonData)
		{
			if (jsonData == null || (!jsonData.IsInt && !jsonData.IsDouble))
			{
				return -1f;
			}

			return jsonData.IsInt ? (int)jsonData : (float)((double)jsonData);
		}

		/// <summary>
		/// Get the double value of this json. If it's not a double or null it will return -1f.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <returns>Double value or -1 if not a double</returns>
		public static double GetDouble(this JsonData jsonData)
		{
			if (jsonData == null)
			{
				return -1.0;
			}
			else if (jsonData.IsDouble)
			{
				return (double)jsonData;
			}
			else if (jsonData.IsInt)
			{
				return (int)jsonData;
			}
			else if (jsonData.IsLong)
			{
				return (long)jsonData;
			}
			else
			{
				return -1.0;
			}
		}

		/// <summary>
		/// Get the double value of this json. If it's not a double or null it will return -1f.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="key">key to look for</param>
		/// <returns>Double value or -1 if not a double</returns>
		public static double GetDouble(this JsonData jsonData, string key)
		{
			return Get(jsonData, key).GetDouble();
		}

		/// <summary>
		/// Get the long value of this json. If it's not a long or null it will return -1.
		/// Will log a warning if the data is not a long.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <returns>Long value or -1 if not a long</returns>
		public static long GetLong(this JsonData jsonData)
		{
			return GetLong(jsonData, true);
		}

		/// <summary>
		/// Get the long value of this json. If it's not a long or null it will return -1.
		/// Will log a warning if the data is not a long.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="key">key to look for</param>
		/// <returns>Long value or -1 if not a long</returns>
		public static long GetLong(this JsonData jsonData, string key)
		{
			return Get(jsonData, key).GetLong(true);
		}

		/// <summary>
		/// Get the long value of this json. If it's not a long or null it will return -1.
		/// Will log a warning if the data is not a long if the warn flag is set.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="warn">Whether to warn if the data is not a long</param>
		/// <returns>Long value or -1 if not a long</returns>
		public static long GetLong(this JsonData jsonData, bool warn)
		{
			if (jsonData == null || (!jsonData.IsInt && !jsonData.IsLong))
			{
				if (warn)
					FrameworkLogger.Warning("Data is not a long");
				return -1;
			}

			return jsonData.IsInt ? (int)jsonData : (long)jsonData;
		}

		/// <summary>
		/// Get an integer array of this json. If it's not an array or does not contain integers it will return null.
		/// Will log a warning if the data is not valid if the warn flag is set.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="warn">Whether to warn if the data is not valid</param>
		/// <returns>Int array or null if not an int array</returns>
		public static int[] GetIntArray(this JsonData jsonData, bool warn = true)
		{
			if (jsonData == null || !jsonData.IsArray)
			{
				if (warn)
					FrameworkLogger.Warning("Data is not an array");
				return null;
			}


			int count = jsonData.Count;
			int[] intArray = new int[count];
			for (int i = 0; i < count; i++)
			{
				JsonData intData = jsonData[i];

				if (intData == null || !intData.IsInt)
				{
					if (warn)
						FrameworkLogger.Warning("Data is not an int array");
					return null;
				}

				intArray[i] = (int)intData;
			}

			return intArray;
		}

		/// <summary>
		/// Get the string value of this json. If it's not a string or null it will return false.
		/// Will log a warning if the data is not a string if the warn flag is set.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="warn">Whether to warn if the data is not a string</param>
		/// <returns>String value or null if not a string</returns>
		public static string GetString(this JsonData jsonData, bool warn = true)
		{
			if (jsonData == null || !jsonData.IsString)
			{
				if (warn)
					FrameworkLogger.Warning("Data is not a string");
				return null;
			}

			return (string)jsonData;
		}

		/// <summary>
		/// Get the key value from the JsonData and convert to string.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="key">key to look for</param>
		/// <returns>Returns the string for this key or null if not a string</returns>
		public static string GetPropertyString(this JsonData jsonData, string key)
		{
			jsonData = jsonData.Get(key);
			if (jsonData == null || !jsonData.IsString)
			{
				if (!key.Equals("type") && !key.Equals("subtype"))
					FrameworkLogger.Warning("Data is not a string");
				return null;
			}

			return (string)jsonData;
		}

		/// <summary>
		/// Get the boolean value of this json. If it's not a boolean or null it will return false.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="warn">Whether to warn if the data is not a string</param>
		/// <returns>Boolean value or null if not a boolean</returns>
		public static bool GetBoolean(this JsonData jsonData, bool warn = true)
		{
			if (jsonData == null || !jsonData.IsBoolean)
			{
				if (warn)
					FrameworkLogger.Warning("Data is not a boolean");
				return false;
			}

			return (bool)jsonData;
		}

		/// <summary>
		/// Get the boolean value of this json. If it's not a boolean or null it will return false.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="key">key to look for</param>
		/// <returns>Boolean value or null if not a boolean</returns>
		public static bool GetBoolean(this JsonData jsonData, string key)
		{
			return Get(jsonData, key).GetBoolean();
		}

		/// <summary>
		/// Get the key value from the JsonData in a safe way. The jsonData must be an object, not be null and contain the key.
		/// If it's invalid it will return null.
		/// </summary>
		/// <param name="jsonData">JsonData</param>
		/// <param name="key">key to look for</param>
		/// <returns>JsonData for the key or null if not found</returns>
		public static JsonData Get(this JsonData jsonData, string key)
		{
			if (jsonData == null || !jsonData.IsObject)
			{
				return null;
			}

			return jsonData.TryGetValue(key);
		}
	}
}