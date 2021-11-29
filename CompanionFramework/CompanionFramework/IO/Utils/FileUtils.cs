using System;
using System.IO;
using System.Text;
using System.Security;
using CompanionFramework.Core.Log;
using CompanionFramework.Net.Http.Common;

namespace CompanionFramework.IO.Utils
{
	public enum FileSaveResult
	{
		/// <summary>
		/// Success.
		/// </summary>
		Success,
		/// <summary>
		/// Generic failed to save file.
		/// </summary>
		Failed,
		/// <summary>
		/// Permission issue.
		/// </summary>
		MissingPermission,
		/// <summary>
		/// No disk space error was thrown.
		/// </summary>
		NoDiskSpace
	}

	public static class FileUtils
	{
		/// <summary>
		/// The default encoding: <see cref="Encoding.UTF8"/>.
		/// </summary>
		public static readonly Encoding DefaultEncoding = Encoding.UTF8;

		private const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027); // 0x27
		private const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070); // 0x70

		#region String Encoding
		/// <summary>
		/// Get a string from a buffered byte array using UTF8 encoding. Returns null if an exception occurred.
		/// </summary>
		/// <param name="data">byte array</param>
		/// <param name="index">index to start</param>
		/// <param name="count">amount of bytes</param>
		/// <returns>string from bytes or null</returns>
		public static string GetString(byte[] data, int index, int count)
		{
			try
			{
				return DefaultEncoding.GetString(data, index, count);
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}

		/// <summary>
		///  Get string from a byte array using UTF8 encoding. Returns null if an exception occurred.
		/// </summary>
		/// <param name="data">byte array</param>
		/// <returns>string from bytes or null</returns>
		public static string GetString(byte[] data)
		{
			try
			{
				return DefaultEncoding.GetString(data);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Get string from a byte array using the specified encoding. Returns null if an exception occurred.
		/// </summary>
		/// <param name="data">byte array</param>
		/// <param name="encoding">encoding to convert</param>
		/// <returns>string from bytes or null</returns>
		public static string GetString(byte[] data, Encoding encoding)
		{
			try
			{
				return encoding.GetString(data);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Get bytes from a string using UTF8 encoding. Returns null if an exception occurred.
		/// </summary>
		/// <param name="text">text to convert</param>
		/// <returns>byte array of the text or null</returns>
		public static byte[] GetBytes(string text)
		{
			try
			{
				return DefaultEncoding.GetBytes(text);
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion

		#region Uri methods
		/// <summary>
		/// Get the directory from a uri. This converts the string to a <see cref="Uri"/> and then strips the file name off the end to get the directory.
		/// Can return null if not a valid uri. Returns the whole uri if it does not have a file name at the end.
		/// </summary>
		/// <param name="uriLocation">Uri location to convert</param>
		/// <returns>Directory from the uri</returns>
		public static string GetDirectoryFromUri(string uriLocation)
		{
			Uri uri = GetSafeUri(uriLocation);
			if (uri == null)
				return null;

			string fileName = GetFileNameFromUri(uri);
			if (fileName != null && !string.IsNullOrEmpty(Path.GetExtension(fileName)))
			{
				return uriLocation.Substring(0, uriLocation.Length - fileName.Length);
			}
			else
			{
				return uri.ToString();
			}
		}

		/// <summary>
		/// Get the directory from a path. If it does not have an extension it appends a directory separator and gets the directory from that.
		/// </summary>
		/// <param name="path">Path to convert</param>
		/// <returns>Directory from the path</returns>
		public static string GetDirectoryFromPath(string path)
		{
			string extension = Path.GetExtension(path);
			if (string.IsNullOrEmpty(extension))
				path = path + Path.DirectorySeparatorChar;

			string directoryPath = Path.GetDirectoryName(path);
			return directoryPath;
		}

		/// <summary>
		/// Get the file name from a uri string. Can be null if the string passed is not a <see cref="Uri"/>.
		/// </summary>
		/// <param name="uriString">String to get the filename from</param>
		/// <returns>Filename or null</returns>
		public static string GetFileNameFromUriString(string uriString)
		{
			Uri uri = GetSafeUri(uriString);
			return GetFileNameFromUri(uri);
		}

		/// <summary>
		/// Simply grabs the extension by finding the period and returning it. This includes the period. Returns an empty string if there's no extension.
		/// </summary>
		/// <param name="fileName">Filename to get extension from</param>
		/// <returns>Extension from the period</returns>
		public static string GetExtension(string fileName)
		{
			int index = fileName.LastIndexOf('.');
			if (index == -1)
				return "";

			return fileName.Substring(index);
		}

		/// <summary>
		/// Get an extension from the filename. This includes the period. Returns an empty string if the filename is invalid.
		/// </summary>
		/// <param name="fileName">Filename to get extension from</param>
		/// <returns>Extension from the period</returns>
		public static string GetExtensionSafe(string fileName)
		{
			try
			{
				return Path.GetExtension(fileName);
			}
			catch (ArgumentException)
			{
				return "";
			}
		}

		/// <summary>
		/// Get an extension from the filename without the extension. Returns an empty string if the filename is invalid.
		/// </summary>
		/// <param name="url">url</param>
		/// <returns>Filename without extension</returns>
		public static string GetFileNameWithoutExtensionSafe(string url)
		{
			try
			{
				return Path.GetFileNameWithoutExtension(url);
			}
			catch (ArgumentException)
			{
				return "";
			}
		}

		/// <summary>
		/// Remove the file extension from the string by finding the last period and removing that.
		/// </summary>
		/// <param name="fileName">Filename to strip extension from</param>
		/// <returns>Filename without extension</returns>
		public static string StripExtension(string fileName)
		{
			int index = fileName.LastIndexOf('.');
			if (index == -1)
				return fileName;

			return fileName.Substring(0, index);
		}

		/// <summary>
		/// Get filename from a <see cref="Uri"/> by returning the last segment.
		/// </summary>
		/// <param name="uri">Uri to get filename from</param>
		/// <returns>Filename from uri or null if no segments are present</returns>
		public static string GetFileNameFromUri(Uri uri)
		{
			if (uri == null || uri.Segments.Length == 0)
				return null;

			return uri.Segments[uri.Segments.Length - 1];
		}

		/// <summary>
		/// Convert a string to a <see cref="Uri"/> which returns null if it cannot convert.
		/// </summary>
		/// <param name="uri">String to convert</param>
		/// <returns>Uri or null if it failed</returns>
		public static Uri GetSafeUri(string uri)
		{
			if (uri == null)
				return null;

			try
			{
				return new Uri(uri);
			}
			catch (UriFormatException e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}
		#endregion

		#region File IO

		
		/// <summary>
		/// Read a text file completely using UTF8. Returns null if the file does not exist. This uses File.ReadAllText.
		/// </summary>
		/// <param name="path">Path to read</param>
		/// <returns>Text file or null if it does not exist</returns>
		public static string ReadTextFileSimple(string path) // seems to be slightly slower than ReadTextFile below, see: IOTest
		{
			if (path == null || !File.Exists(path))
				return null;

			return File.ReadAllText(path, DefaultEncoding);
		}

		/// <summary>
		/// Read a text file completely using UTF8. Returns null if the file does not exist.
		/// </summary>
		/// <param name="path">Path to read</param>
		/// <returns>Text file or null if it does not exist</returns>
		public static string ReadTextFile(string path)
		{
			byte[] fileData = ReadFileSimple(path);
			return fileData != null ? GetString(fileData) : null;
		}

		/// <summary>
		/// Read a text file completely using the specified encoding. Returns null if the file does not exist.
		/// </summary>
		/// <param name="path">Path to read</param>
		/// <param name="encoding">Encoding to use</param>
		/// <returns>Text file or null if it does not exist</returns>
		public static string ReadTextFile(string path, Encoding encoding)
		{
			byte[] fileData = ReadFile(path);
			return fileData != null ? GetString(fileData, encoding) : null;
		}

		/// <summary>
		/// Read a binary file completely. Returns null if the file does not exist. This uses File.ReadAllbytes.
		/// </summary>
		/// <param name="path">Path to read</param>
		/// <returns>Byte array or null if it does not exist</returns>
		public static byte[] ReadFileSimple(string path)
		{
			if (path == null || !File.Exists(path))
				return null;

			return File.ReadAllBytes(path);
		}

		/// <summary>
		/// Buffered read a binary file completely. Returns null if the file does not exist.
		/// </summary>
		/// <param name="path">Path to read</param>
		/// <param name="progress">Optional progress tracker</param>
		/// <returns>Byte array or null if it does not exist</returns>
		public static byte[] ReadFile(string path, IStreamProgress progress = null) // this is slower than File.ReadAllBytes by a few milliseconds, see: IOTest
		{
			if (path == null || !File.Exists(path))
				return null;

			FileStream stream = null;
			try
			{
				stream = new FileStream(path, FileMode.Open);
				return FileUtils.GetByteArrayFromStream(stream, progress);
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
		}

		/// <summary>
		/// Read the whole stream of bytes and return the array.
		/// </summary>
		/// <param name="stream">Stream to read from</param>
		/// <param name="progress">Optional progress tracker</param>
		/// <param name="contentLength">Content length or zero if not provided</param>
		/// <returns>Byte array or null if the stream is null</returns>
		public static byte[] GetByteArrayFromStream(Stream stream, IStreamProgress progress = null, long contentLength = 0)
		{
			if (stream == null)
				return null;

			MemoryStream writer = null;
			try
			{
				// Write to file
				byte[] buffer = new byte[4096];

				int bytesRead = stream.Read(buffer, 0, buffer.Length);
				long totalBytes = 0;
				while (bytesRead > 0)
				{
					if (writer == null)
						writer = new MemoryStream(bytesRead);

					if (progress != null)
					{
						totalBytes += bytesRead;
						progress.UpdateProgress(totalBytes, contentLength);
						//FrameworkLogger.Message("Progress: " + totalBytes + " bytes read: " + bytesRead);
					}

					writer.Write(buffer, 0, bytesRead);
					bytesRead = stream.Read(buffer, 0, buffer.Length);
				}

				if (writer == null)
					return null;

				return writer.ToArray();
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
		}

		/// <summary>
		/// Save a byte array to the specified path synchronously.
		/// </summary>
		/// <param name="data">Byte array to save</param>
		/// <param name="path">Path to save to</param>
		/// <returns>FileSaveResult.Success if save succeeded</returns>
		public static FileSaveResult Save(byte[] data, string path)
		{
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(path, FileMode.Create);
				fileStream.Write(data, 0, data.Length);
				return FileSaveResult.Success;
			}
			catch (IOException e)
			{
				FrameworkLogger.Exception(e);

				if (IsExceptionDiskFull(e))
				{
					return FileSaveResult.NoDiskSpace;
				}
				else
				{
					return FileSaveResult.Failed;
				}
			}
			catch (SecurityException e)
			{
				FrameworkLogger.Exception(e);
				return FileSaveResult.MissingPermission;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return FileSaveResult.Failed;
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
		}

		/// <summary>
		/// Save a string to the specified path synchronously.
		/// </summary>
		/// <param name="text">String to save</param>
		/// <param name="path">Path to save to</param>
		/// <returns>FileSaveResult.Success if save succeeded</returns>
		public static FileSaveResult SaveTextFile(string text, string path)
		{
			byte[] data = GetBytes(text);

			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(path, FileMode.Create);
				fileStream.Write(data, 0, data.Length);
				return FileSaveResult.Success;
			}
			catch (IOException e)
			{
				FrameworkLogger.Exception(e);

				if (IsExceptionDiskFull(e))
				{
					return FileSaveResult.NoDiskSpace;
				}
				else
				{
					return FileSaveResult.Failed;
				}
			}
			catch (SecurityException e)
			{
				FrameworkLogger.Exception(e);
				return FileSaveResult.MissingPermission;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return FileSaveResult.Failed;
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
		}

		/// <summary>
		/// Save a byte array to the specified path asynchronously.
		/// </summary>
		/// <param name="data">Byte array to save</param>
		/// <param name="path">Path to save to</param>
		/// <returns>Async result or null if failed</returns>
		public static IAsyncResult SaveAsync(byte[] data, string path)
		{
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(path, FileMode.Create);
				return fileStream.BeginWrite(data, 0, data.Length, EndSave, fileStream);
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);

				// only close if an exception occurred
				if (fileStream != null)
					fileStream.Close();
			}

			return null;
		}

		/// <summary>
		/// Finish the async saving of the byte array.
		/// </summary>
		/// <param name="result">result</param>
		private static void EndSave(IAsyncResult result)
		{
			FileStream fileStream = result.AsyncState as FileStream;
			using (fileStream)
			{
				try
				{
					fileStream.EndWrite(result);
				}
				catch (Exception e)
				{
					FrameworkLogger.Exception(e);
					return;
				}
			}
		}

		/// <summary>
		/// Check if the exception is because of the disk being full. This checks the error result and works on .net 4.6+ only.
		/// </summary>
		/// <param name="e">Exception to check</param>
		/// <returns>True if exception was caused by disk being full</returns>
		public static bool IsExceptionDiskFull(Exception e)
		{
			if (e == null)
				return false;

#if NET46 || NET_STANDARD_2_0
			int errorCode = e.HResult;
			return errorCode == HR_ERROR_HANDLE_DISK_FULL || errorCode == HR_ERROR_DISK_FULL;
#else
			return false;
#endif
		}
		#endregion
	}
}