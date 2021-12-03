using CompanionFramework.Core.Log;
using CompanionFramework.Json.Extensions;
using LitJson;
using System;

namespace Companion.Data.System.Update
{
	public class Repository
	{
		public string name;
		public string description;
		public string battleScribeVersion;
		public string version;
		public string lastUpdated;
		public string lastUpdateDescription;
		public string indexUrl;
		public string repositoryUrl;
		public string githubUrl;
		public string feedUrl;
		public string bugTrackerUrl;
		public string reportBugUrl;
		public bool archived;
		
		// implement repository files which seems to just always be an empty array?

		public Repository(JsonData jsonData)
		{
			name = jsonData.Get("name").GetString();
			description = jsonData.Get("description").GetString();
			battleScribeVersion = jsonData.Get("battleScribeVersion").GetString();
			version = jsonData.Get("version").GetString();
			lastUpdated = jsonData.Get("lastUpdated").GetString();
			lastUpdateDescription = jsonData.Get("lastUpdateDescription").GetString();
			indexUrl = jsonData.Get("indexUrl").GetString();
			repositoryUrl = jsonData.Get("repositoryUrl").GetString();
			githubUrl = jsonData.Get("githubUrl").GetString();
			feedUrl = jsonData.Get("feedUrl").GetString();
			bugTrackerUrl = jsonData.Get("bugTrackerUrl").GetString();
			reportBugUrl = jsonData.Get("reportBugUrl").GetString();
			archived = jsonData.Get("archived").GetBoolean();
		}

		public static Repository Parse(JsonData jsonData)
		{
			try
			{
				Repository repository = new Repository(jsonData);
				return repository;
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
				return null;
			}
		}
	}
}
