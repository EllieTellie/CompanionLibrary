using CompanionFramework.Core.Log;
using CompanionFramework.Json.Extensions;
using LitJson;
using System;

namespace Companion.Data.System.Update
{
	/// <summary>
	/// Holds the repository information retrieved 
	/// </summary>
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

		/// <summary>
		/// Create a repository from the json provided.
		/// </summary>
		/// <param name="jsonData">Json data</param>
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

		/// <summary>
		/// Write this object out as a json object.
		/// </summary>
		/// <param name="writer">Json Writer to use</param>
		public void Write(JsonWriter writer)
		{
			writer.WriteObjectStart();

			writer.WritePropertyName("name");
			writer.Write(name);
			writer.WritePropertyName("description");
			writer.Write(description);
			writer.WritePropertyName("battleScribeVersion");
			writer.Write(battleScribeVersion);
			writer.WritePropertyName("version");
			writer.Write(version);
			writer.WritePropertyName("lastUpdated");
			writer.Write(lastUpdated);
			writer.WritePropertyName("lastUpdateDescription");
			writer.Write(lastUpdateDescription);
			writer.WritePropertyName("indexUrl");
			writer.Write(indexUrl);
			writer.WritePropertyName("repositoryUrl");
			writer.Write(repositoryUrl);
			writer.WritePropertyName("githubUrl");
			writer.Write(githubUrl);
			writer.WritePropertyName("feedUrl");
			writer.Write(feedUrl);
			writer.WritePropertyName("bugTrackerUrl");
			writer.Write(bugTrackerUrl);
			writer.WritePropertyName("reportBugUrl");
			writer.Write(reportBugUrl);
			writer.WritePropertyName("archived");
			writer.Write(archived);

			writer.WriteObjectEnd();
		}

		public override string ToString()
		{
			return name;
		}

		public string GetRepositoryUrl()
		{
			if (repositoryUrl.EndsWith("/"))
				return repositoryUrl;
			else
				return repositoryUrl + "/";
		}
	}
}
