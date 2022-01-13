using CompanionFramework.Core.Log;
using System;
using System.IO;
using System.IO.Compression;
using System.Security;

namespace CompanionFramework.IO.Utils
{
	public static class CompressionUtils
	{
		/// <summary>
		/// Extract byte array to the specified path. Uses GZip.
		/// </summary>
		/// <param name="data">byte array to unzip</param>
		/// <param name="outputPath">path to save to</param>
		/// <returns>FileSaveResult.Success if successful</returns>
		public static FileSaveResult DecompressGZipToPath(byte[] data, string outputPath)
		{
			try
			{
				using (FileStream decompressedFileStream = new FileStream(outputPath, FileMode.Create))
				{
					using (GZipStream decompressionStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
					{
						byte[] buffer = new byte[4096];

						int bytesRead = decompressionStream.Read(buffer, 0, buffer.Length);
						if (bytesRead == 0)
							return FileSaveResult.Failed; // likely to be wrong if the initial read is zero

						while (bytesRead > 0)
						{
							// write to output file
							decompressedFileStream.Write(buffer, 0, bytesRead);
							bytesRead = decompressionStream.Read(buffer, 0, buffer.Length);
						}
					}
				}

				return FileSaveResult.Success;
			}
			catch (IOException e)
			{
				FrameworkLogger.Exception(e);

				if (FileUtils.IsExceptionDiskFull(e))
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
		}

		/// <summary>
		/// Decompress a .zip file into a byte[] data. The file inside the zip must have the extension provided.
		/// </summary>
		/// <param name="data">Data</param>
		/// <param name="extension">extension of the filename in the zip</param>
		/// <returns>Decompressed data</returns>
		public static byte[] DecompressFileFromZip(byte[] data, string extension)
		{
			byte[] uncompressedData = null;
			using (MemoryStream zipStream = new MemoryStream(data))
			{
				using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
				{
					foreach (ZipArchiveEntry entry in archive.Entries)
					{
						if (entry.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
						{
							using (Stream stream = entry.Open())
							{
								if (stream != null)
								{
									uncompressedData = FileUtils.GetByteArrayFromStream(stream, null);
									if (uncompressedData != null)
										break;
								}
							}
						}
					}
				}
			}

			return uncompressedData;
		}
	}
}
