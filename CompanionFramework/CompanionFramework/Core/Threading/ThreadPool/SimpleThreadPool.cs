using CompanionFramework.Core.Log;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CompanionFramework.Core.Threading.ThreadPool
{
	/// <summary>
	/// Simple thread pool that can store multiple threads and execute tasks on the threads.
	/// </summary>
	public class SimpleThreadPool : IDisposable // Loosely based on: http://www.albahari.com/threading/part4.aspx#_Wait_Pulse_Producer_Consumer_Queue
	{
		private readonly object lockObject = new Object();

		private List<Thread> threads;
		private Queue<Action> queue = new Queue<Action>();

		private int workerThreads;
		private bool isDestroyed = false;

		private bool ready;
		private bool disposed;

		/// <summary>
		/// Whether the thread pool is destroyed.
		/// </summary>
		public bool Destroyed
		{
			get { return isDestroyed; }
		}

		/// <summary>
		/// Create a thread pool with a maximum capacity of 1.
		/// </summary>
		public SimpleThreadPool() : this(1) { }

		/// <summary>
		/// Create a thread pool with a specified maximum capacity.
		/// </summary>
		/// <param name="workerThreads">How many threads to create</param>
		public SimpleThreadPool(int workerThreads)
		{
			this.threads = new List<Thread>(workerThreads);
			this.workerThreads = workerThreads;

			SpawnWorkerThreads();
		}

		/// <summary>
		/// Add a task to the queue for processing, helper method that passes ITask.Run to Add()
		/// </summary>
		/// <param name="task">The task to be processed</param>
		/// <returns>true if the task was added</returns>
		public bool Add(IBaseThreadedTask task)
		{
			return Add(task.Run);
		}

		/// <summary>
		/// Add an action to the queue for processing
		/// </summary>
		/// <param name="action">The action to be processed</param>
		/// <returns>true if the action was added</returns>
		public bool Add(Action action)
		{
			if (action == null || isDestroyed)
				return false;

			lock (lockObject)
			{
				queue.Enqueue(action);
				Monitor.Pulse(lockObject);
			}

			return true;
		}

		/// <summary>
		/// Creates the initial threads. This could be improved to lazily init or spawn more as desired.
		/// </summary>
		private void SpawnWorkerThreads()
		{
			// spawn worker threads
			for (int i = 0; i < workerThreads; i++)
			{
				Thread thread = new Thread(Run);
				thread.IsBackground = true;
				threads.Add(thread);
				thread.Start();
			}
		}

		/// <summary>
		/// Destroy the thread pool, don't wait on the threads to finish
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			// always do this
			Destroy(false);

			disposed = true;
		}

		~SimpleThreadPool()
		{
			Dispose(false);
		}

		/// <summary>
		/// Destroy the thread pool.
		/// </summary>
		/// <param name="waitOnThreads">Whether to wait on all running threads to complete.</param>
		public void Destroy(bool waitOnThreads)
		{
			lock (lockObject)
			{
				isDestroyed = true;
				queue.Clear();
				Monitor.PulseAll(lockObject); // pulse all to release threads
			}

			if (waitOnThreads)
			{
				foreach (Thread thread in threads)
				{
					if (thread.IsAlive)
						thread.Join();
				}
			}
		}

		/// <summary>
		/// How many tasks are currently queued.
		/// </summary>
		/// <returns>Queue count</returns>
		public int Count()
		{
			lock (lockObject)
			{
				return queue.Count;
			}
		}

		protected void Run()
		{
			while (!isDestroyed)
			{
				Action action;
				lock (lockObject)
				{
					while (queue.Count == 0)
					{
						if (!ready)
						{
							ready = true;
							Monitor.PulseAll(lockObject);
						}

						Monitor.Wait(lockObject); // release lock and wait for a pulse

						// check the destroyed flag and exit the thread if destroyed
						if (isDestroyed)
							return;
					}

					// grab the next action
					action = queue.Dequeue();
				}

				// if the action is null exit the thread
				if (action == null)
					return;

				// run the action
				try
				{
					action();
				}
				catch (OperationCanceledException)
				{
					// write debug message
				}
				catch (Exception e)
				{
					FrameworkLogger.Exception(e);
				}
			}
		}
	}
}