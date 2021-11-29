using System;
using System.Collections.Generic;
using System.IO;

namespace CompanionFramework.IO.Utils
{
	/// <summary>
	/// Used for file searching. Added so tools can benefit from it without having to copy the class.
	/// </summary>
	public static class FileSearchUtils
	{
		/// <summary>
		/// Find file names by extension. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="searchExtension">Search extension</param>
		/// <param name="maxDepth">Max Depth</param>
		/// <returns>Returns the list of files</returns>
		public static List<string> FindFileNamesByExtension(string searchDirectory, string searchExtension, int maxDepth = 2)
		{
			if (searchDirectory == null)
				throw new ArgumentNullException("searchDirectory", "Search directory cannot be null");
			else if (searchExtension == null)
				throw new ArgumentNullException("searchExtension", "Search extension cannot be null");

			List<string> results = new List<string>();
			FindFileNamesByExtension(results, searchDirectory, searchExtension, 0, maxDepth);
			return results;
		}

		// recursive function to execute the above
		private static void FindFileNamesByExtension(List<string> results, string searchDirectory, string searchExtension, int depth, int maxDepth)
		{
			string[] files = Directory.GetFiles(searchDirectory);

			foreach (string file in files)
			{
				string extension = Path.GetExtension(file);

				if (!string.IsNullOrEmpty(extension) && extension == searchExtension)
				{
					results.Add(file);
				}
			}

			string[] directories = Directory.GetDirectories(searchDirectory);
			foreach (string directory in directories)
			{
				//string[] files = Directory.GetFiles(directory);

				//foreach (string file in files)
				//{
				//	string extension = Path.GetExtension(file);

				//	if (!string.IsNullOrEmpty(extension) && extension == searchExtension)
				//	{
				//		results.Add(file);
				//	}
				//}

				int searchDepth = depth + 1; // make it easier to understand max depth by increasing it first before checking
				if (searchDepth < maxDepth)
				{
					FindFileNamesByExtension(results, directory, searchExtension, searchDepth, maxDepth);
				}
			}
		}

		/// <summary>
		/// Find file by exact file name. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="searchName">Search name</param>
		/// <param name="maxDepth">Max Depth</param>
		/// <returns>Returns the file</returns>
		public static string FindFileName(string searchDirectory, string searchName, int maxDepth = 2)
		{
			if (searchDirectory == null)
				throw new ArgumentNullException("searchDirectory", "Search directory cannot be null");
			else if (searchName == null)
				throw new ArgumentNullException("searchName", "Search name cannot be null");
			
			return FindFileName(searchDirectory, searchName, 0, maxDepth);
		}

		// recursive function to execute the above
		private static string FindFileName(string searchDirectory, string searchName, int depth, int maxDepth)
		{
			string fileName = Path.Combine(searchDirectory, searchName);
			if (File.Exists(fileName))
			{
				return fileName;
			}

			string[] directories = Directory.GetDirectories(searchDirectory);
			foreach (string directory in directories)
			{
				int searchDepth = depth + 1; // make it easier to understand max depth by increasing it first before checking
				if (searchDepth < maxDepth)
				{
					string file = FindFileName(directory, searchName, searchDepth, maxDepth);
					if (file != null)
						return file;
				}
			}

			return null;
		}

		/// <summary>
		/// Find file names by exact file name. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="searchName">Search name</param>
		/// <param name="maxDepth">Max Depth</param>
		/// <returns>Returns the list of files</returns>
		public static List<string> FindFileNames(string searchDirectory, string searchName, int maxDepth = 2)
		{
			if (searchDirectory == null)
				throw new ArgumentNullException("searchDirectory", "Search directory cannot be null");
			else if (searchName == null)
				throw new ArgumentNullException("searchName", "Search name cannot be null");

			List<string> results = new List<string>();
			FindFileNames(results, searchDirectory, searchName, 0, maxDepth);
			return results;
		}

		// recursive function to execute the above
		private static void FindFileNames(List<string> results, string searchDirectory, string searchName, int depth, int maxDepth)
		{
			string[] directories = Directory.GetDirectories(searchDirectory);
			foreach (string directory in directories)
			{
				string fileName = Path.Combine(directory, searchName);

				if (File.Exists(fileName))
				{
					results.Add(fileName);
				}
				else
				{
					int searchDepth = depth + 1; // make it easier to understand max depth by increasing it first before checking
					if (searchDepth < maxDepth)
					{
						FindDirectoryNames(results, directory, searchName, searchDepth, maxDepth);
					}
				}
			}
		}

		/// <summary>
		/// Find directories by exact directory name. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="directoryName">Search name</param>
		/// <param name="maxDepth">Max Depth</param>
		/// <returns>Returns the list of files</returns>
		public static List<string> FindDirectoryNames(string searchDirectory, string directoryName, int maxDepth = 2)
		{
			if (searchDirectory == null)
				throw new ArgumentNullException("searchDirectory", "Search directory cannot be null");
			else if (directoryName == null)
				throw new ArgumentNullException("directoryName", "Directory name cannot be null");

			List<string> results = new List<string>();
			FindDirectoryNames(results, searchDirectory, directoryName, 0, maxDepth);
			return results;
		}

		// recursive function to execute the above
		private static void FindDirectoryNames(List<string> results, string searchDirectory, string directoryName, int depth, int maxDepth)
		{
			string[] directories;
			try
			{
				directories = Directory.GetDirectories(searchDirectory);
			}
			catch (UnauthorizedAccessException)
			{
				return;
			}

			foreach (string directory in directories)
			{
				string fileName = Path.GetFileName(directory);

				if (fileName == directoryName)
				{
					results.Add(directory);
				}
				else
				{
					int searchDepth = depth + 1; // make it easier to understand max depth by increasing it first before checking
					if (searchDepth < maxDepth)
					{
						FindDirectoryNames(results, directory, directoryName, searchDepth, maxDepth);
					}
				}
			}
		}

		/// <summary>
		/// Find the first directory name inside the search directory. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="directoryName">Directory name to search for</param>
		/// <param name="maxDepth">Max Depth</param>
		/// <returns>Directory name if found</returns>
		public static string FindDirectoryName(string searchDirectory, string directoryName, int maxDepth = 2)
		{
			if (searchDirectory == null)
				throw new ArgumentNullException("searchDirectory", "Search directory cannot be null");
			else if (directoryName == null)
				throw new ArgumentNullException("directoryName", "Directory name cannot be null");

			return FindDirectoryName(searchDirectory, directoryName, 0, maxDepth);
		}

		// recursive function to execute the above
		private static string FindDirectoryName(string searchDirectory, string directoryName, int depth, int maxDepth)
		{
			string[] directories = Directory.GetDirectories(searchDirectory);
			foreach (string directory in directories)
			{
				string fileName = Path.GetFileName(directory);

				if (fileName == directoryName)
				{
					return directory;
				}
				else
				{
					int searchDepth = depth + 1;
					if (searchDepth < maxDepth)
					{
						string found = FindDirectoryName(directory, directoryName, searchDepth, maxDepth);
						if (found != null)
							return found;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Find the first directory that contains all provided directories. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="directoryNames">Directory names that must be present</param>
		/// <returns>Directory name if found</returns>
		public static string FindMatchingDirectories(string searchDirectory, params string[] directoryNames)
		{
			return FindMatchingDirectories(searchDirectory, 2, directoryNames);
		}

		/// <summary>
		/// Find the first directory name inside the search directory. This recursively searches directories until max depth is reached. By default it searches the first directory and the next directory.
		/// </summary>
		/// <param name="searchDirectory">Search directory</param>
		/// <param name="maxDepth">Max Depth</param>
		/// <param name="directoryNames">Directory names to search for</param>
		/// <returns>Directory name if found</returns>
		public static string FindMatchingDirectories(string searchDirectory, int maxDepth, params string[] directoryNames)
		{
			if (searchDirectory == null)
				throw new ArgumentNullException("searchDirectory", "Search directory cannot be null");
			else if (directoryNames == null)
				throw new ArgumentNullException("directoryNames", "Directory names cannot be null");

			return FindMatchingDirectories(searchDirectory, 0, maxDepth, directoryNames);
		}

		// recursive function to execute the above
		private static string FindMatchingDirectories(string searchDirectory, int depth, int maxDepth, string[] directoryNames)
		{
			bool match = true;
			foreach (string dir in directoryNames)
			{
				string test = Path.Combine(searchDirectory, dir);
				if (!Directory.Exists(test))
				{
					match = false;
					break;
				}
			}

			if (match)
			{
				return searchDirectory;
			}
			else
			{
				int searchDepth = depth + 1;
				if (searchDepth < maxDepth)
				{
					// search subdirectories
					string[] directories = Directory.GetDirectories(searchDirectory);
					foreach (string directory in directories)
					{
						string found = FindMatchingDirectories(directory, searchDepth, maxDepth, directoryNames);
						if (found != null)
							return found;
					}
				}
			}

			return null;
		}
	}
}
