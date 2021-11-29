//#define PERFORMANCE_PROFILING

using System;
#if PERFORMANCE_PROFILING
using System.Diagnostics;
#endif

namespace CompanionFramework.Core.Threading.Messaging
{
	/// <summary>
	/// Wrapper for source and event arguments. Copies the <see cref="EventHandler"/> to avoid modifications to the list.
	/// </summary>
	public class MessageEvent
	{
		private object source;
		private EventArgs eventArgs;
		private event EventHandler StoredEvent;

#if PERFORMANCE_PROFILING
	private static readonly Stopwatch stopwatch = new Stopwatch();
#endif

		public MessageEvent(EventHandler theEvent, object source, EventArgs eventArgs)
		{
			this.source = source;
			this.eventArgs = eventArgs;
			this.StoredEvent = theEvent;
		}

		/// <summary>
		/// Get the event source.
		/// </summary>
		/// <returns>Event source</returns>
		public object GetSource()
		{
			return source;
		}

		/// <summary>
		/// Get the event arguments.
		/// </summary>
		/// <returns>Event arguments</returns>
		public EventArgs GetEventArgs()
		{
			return eventArgs;
		}

		/// <summary>
		/// Dispatch the event. This executes the event immediately.
		/// </summary>
		public void Dispatch()
		{
#if PERFORMANCE_PROFILING
			stopwatch.Reset();
			stopwatch.Start();
#endif

			StoredEvent(source, eventArgs);

#if PERFORMANCE_PROFILING
			stopwatch.Stop();

			if (stopwatch.ElapsedMilliseconds > 15)
			{
				Delegate[] delegateList = StoredEvent.GetInvocationList();

				FrameworkLogger.Warning("Event took >15ms, calls: " + delegateList.Length + " time: " + stopwatch.ElapsedMilliseconds);

				for (int i = 0; i < delegateList.Length; i++)
				{
					Delegate @delegate = delegateList[i];
						FrameworkLogger.Warning("Called: " + @delegate.Target.GetType().Name + " Method: " + @delegate.Method.Name);
				}
			}
#endif
		}
	}
}