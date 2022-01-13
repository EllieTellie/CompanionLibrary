using CompanionFramework.Core.Log;
using CompanionFramework.Core.Threading.Messaging;
using CompanionFramework.Core.Threading.ThreadPool;
using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Companion.Data
{
	/// <summary>
	/// Track loading the game system.
	/// </summary>
	public class GameSystemLoading
	{
		protected readonly string gameSystemPath;
		protected List<string> catalogueFilePaths;

		protected GameSystem gameSystem;

		protected List<Catalogue> catalogues = new List<Catalogue>();

		protected bool completed;
		protected bool failed;

		/// <summary>
		/// Fired when progress update is due. This is on the main thread if <see cref="MessageQueue"/> is supported.
		/// </summary>
		public event Action<float, float> OnProgressUpdate;

		/// <summary>
		/// Fired when completed. Source is this.
		/// </summary>
		public event EventHandler OnLoadingCompleted;

		/// <summary>
		/// Fired when failed. Source is this.
		/// </summary>
		public event EventHandler OnLoadingFailed;

		public GameSystemLoading(string gameSystemPath)
		{
			this.gameSystemPath = gameSystemPath;
		}

		public void Run()
		{
			try
			{
				// find catalogue files
				string directory = FileUtils.GetDirectoryFromPath(gameSystemPath);
				catalogueFilePaths = FileSearchUtils.FindFileNamesByExtension(directory, ".catz", 1);

				FrameworkLogger.Message("Found catalogues: " + catalogueFilePaths.Count);

				ThreadPool.QueueUserWorkItem(LoadFullGameSystem);
			}
			catch (Exception e)
			{
				FrameworkLogger.Exception(e);
			}
		}

		private void LoadFullGameSystem(object state)
		{
			gameSystem = GameSystem.LoadGameSystem(gameSystemPath);

			foreach (string path in catalogueFilePaths)
			{
				Catalogue catalogue;
				try
				{
					catalogue = Catalogue.LoadCatalogue(path);
				}
				catch (Exception e) // catch any exception as there is a fair amount of I/O above
				{
					FrameworkLogger.Exception(e);
					catalogue = null; // force fail
				}

				UpdateProgress();

				if (catalogue == null)
				{
					Failed();
					return;
				}

				catalogues.Add(catalogue);
			}

			if (gameSystem != null && catalogues.Count == catalogueFilePaths.Count)
			{
				Completed();
			}
			else
			{
				Failed();
			}
		}

		/// <summary>
		/// Get the game system group. This never returns null. If this has failed game system or catalogues may not be populated.
		/// </summary>
		/// <returns></returns>
		public GameSystemGroup GetGameSystemGroup()
		{
			return new GameSystemGroup(gameSystem, catalogues);
		}

		private void UpdateProgress()
		{
			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnUpdateProgressMainThread);
			}
			else
			{
				OnUpdateProgressMainThread(null, null);
			}
		}

		private void OnUpdateProgressMainThread(object sender, EventArgs e)
		{
			int maximum = catalogueFilePaths.Count + 1; // + 1 for the game system

			int value = 0;

			if (gameSystem != null)
				value++;

			value += catalogues.Count;

			if (OnProgressUpdate != null)
				OnProgressUpdate(value, maximum);
		}

		private void Failed()
		{
			FrameworkLogger.Message("Failed");

			if (failed)
				return;

			failed = true;

			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnLoadingFailed, this);
			}
			else
			{
				if (OnLoadingFailed != null)
					OnLoadingFailed(this, null);
			}
		}

		private void Completed()
		{
			FrameworkLogger.Message("Completed");

			completed = true;

			if (MessageHandler.HasMessageHandler())
			{
				MessageQueue.Invoke(OnLoadingCompleted, this);
			}
			else
			{
				if (OnLoadingCompleted != null)
					OnLoadingCompleted(this, null);
			}
		}
	}
}
