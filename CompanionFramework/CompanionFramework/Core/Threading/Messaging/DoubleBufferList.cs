using System;
using System.Collections.Generic;

namespace CompanionFramework.Core.Threading.Messaging
{
	/// <summary>
	/// Attempt at making a thread safe double buffer list. In theory it should allow writes to the list while the list is being read.
	/// </summary>
	/// <typeparam name="T">The type contained in the list</typeparam>
	public class DoubleBufferList<T>
	{
		private object listLock = new Object();

		private List<T> input;
		private List<T> output;

		private bool hasInput = false;

		public DoubleBufferList()
		{
			this.input = new List<T>();
			this.output = new List<T>();
		}

		public DoubleBufferList(int capacity)
		{
			this.input = new List<T>(capacity);
			this.output = new List<T>(capacity);
		}

		/// <summary>
		/// Add to the write list.
		/// </summary>
		/// <param name="data">data to add</param>
		public void Add(T data)
		{
			lock (listLock)
			{
				hasInput = true;
				input.Add(data);
			}
		}

		/// <summary>
		/// Get the list for reading.
		/// </summary>
		/// <returns>List for reading</returns>
		public List<T> Swap()
		{
			lock (listLock)
			{
				//int inputSize = input.Count;

				List<T> tempList = input;
				input = output;
				output = tempList;

				// clear input
				input.Clear();
				hasInput = false;

				//if (output.Count != inputSize)
				//{
				//	FrameworkLogger.Error("CRITICAL: Error swapping in DoubleBufferList");
				//}

				return output;
			}
		}

		/// <summary>
		/// Whether the list has any data.
		/// </summary>
		/// <returns>True if the list has data</returns>
		public bool HasInput()
		{
			return hasInput;
		}
	}
}