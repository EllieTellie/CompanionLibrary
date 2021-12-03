using CompanionFramework.Core.Log;
using CompanionFramework.Json.Extensions;
using LitJson;
using System;
using System.Collections.Generic;

namespace Companion.Data.System.Update
{
	public class RepositoryIndex
	{
		public string name;
		public string description;
		public string battleScribeVersion;
		public string repositorySourceUrl;
		public string websiteUrl;
		public string githubUrl;
		public string discordUrl;
		public string feedUrl;
		public string twitterUrl;
		public string facebookUrl;

		public List<Repository> repositories = new List<Repository>();

		public RepositoryIndex(JsonData jsonData)
		{
			name = jsonData.Get("name").GetString();
			description = jsonData.Get("description").GetString();
			battleScribeVersion = jsonData.Get("battleScribeVersion").GetString();
			repositorySourceUrl = jsonData.Get("repositorySourceUrl").GetString();
			websiteUrl = jsonData.Get("websiteUrl").GetString();
			githubUrl = jsonData.Get("githubUrl").GetString();
			discordUrl = jsonData.Get("discordUrl").GetString();
			feedUrl = jsonData.Get("feedUrl").GetString();
			twitterUrl = jsonData.Get("twitterUrl").GetString();
			facebookUrl = jsonData.Get("facebookUrl").GetString();

			// repositories
			JsonData repositoryData = jsonData.Get("repositories");

			if (repositoryData != null && repositoryData.IsArray)
			{
				for (int i = 0; i < repositoryData.Count; i++)
				{
					JsonData repositoryJson = repositoryData[i];
					if (repositoryJson != null && repositoryJson.IsObject)
					{
						Repository repository = Repository.Parse(repositoryJson);
						if (repository != null)
							repositories.Add(repository);
					}
				}
			}
		}

		public static RepositoryIndex Parse(JsonData jsonData)
		{
			try
			{
				RepositoryIndex repositoryIndex = new RepositoryIndex(jsonData);
				return repositoryIndex;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}
	}
}
