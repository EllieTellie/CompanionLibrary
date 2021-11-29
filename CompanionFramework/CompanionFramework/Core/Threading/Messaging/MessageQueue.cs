using CompanionFramework.Core.Log;
using System;
using System.Collections.Generic;

namespace CompanionFramework.Core.Threading.Messaging
{
	/// <summary>
	/// A simple message queue for throwing events back to the Unity main loop.
	/// </summary>
	public class MessageQueue
	{
		private DoubleBufferList<MessageEvent> queue;
		private bool dispatching;

		public MessageQueue()
		{
			queue = new DoubleBufferList<MessageEvent>();
		}

		/// <summary>
		/// Invoke this event on the unity thread. There may be a delay until the event gets fired.
		/// Note: The source object and event arguments are set to null in this helper method.
		/// </summary>
		/// <param name="invokeEvent">The event to fire</param>
		/// <returns>True if successful</returns>
		public static bool Invoke(EventHandler invokeEvent)
		{
			if (invokeEvent == null)
				return false;

			MessageHandler.Instance().GetMessageQueue().Add(new MessageEvent(invokeEvent, null, null));
			return true;
		}

		/// <summary>
		/// Invoke this event on the unity thread. There may be a delay until the event gets fired.
		/// Note: The event arguments are set to null in this helper method.
		/// </summary>
		/// <param name="invokeEvent">The event to fire</param>
		/// <param name="source">The source object</param>
		/// <returns>True if successful</returns>
		public static bool Invoke(EventHandler invokeEvent, object source)
		{
			if (invokeEvent == null)
				return false;

			MessageHandler.Instance().GetMessageQueue().Add(new MessageEvent(invokeEvent, source, null));
			return true;
		}

		/// <summary>
		/// Invoke this event on the unity thread. There may be a delay until the event gets fired.
		/// </summary>
		/// <param name="invokeEvent">The event to fire</param>
		/// <param name="source">The source object</param>
		/// <param name="eventArgs">The event arguments</param>
		/// <returns>True if successful</returns>
		public static bool Invoke(EventHandler invokeEvent, object source, EventArgs eventArgs)
		{
			if (invokeEvent == null)
				return false;

			MessageHandler.Instance().GetMessageQueue().Add(new MessageEvent(invokeEvent, source, eventArgs));
			return true;
		}

		public static MessageQueue Get()
		{
			return MessageHandler.Instance().GetMessageQueue();
		}

		public void Add(MessageEvent messageEvent)
		{
			queue.Add(messageEvent);
		}

		/// <summary>
		/// Whether the queue has any messages queued.
		/// </summary>
		/// <returns>True if the queue contains messages</returns>
		public bool HasInput()
		{
			return queue.HasInput();
		}

		/// <summary>
		/// Dispatch should be called from a main loop or timer loop. Dispatch processes any messages currently in the queue.
		/// </summary>
		public void Dispatch()
		{
			// return without locking
			if (!queue.HasInput() || dispatching)
				return;

			lock (queue)
			{

				// dispatch all
				dispatching = true;

				List<MessageEvent> eventQueue = queue.Swap();

				for (int i = 0; i < eventQueue.Count; i++)
				{
					MessageEvent messageEvent = eventQueue[i];
					try
					{
						messageEvent.Dispatch();
					}
					catch (Exception e)
					{
						FrameworkLogger.Instance().GetLogger().MessageLogError("Caught exception in message queue.");

						FrameworkLogger.Instance().GetLogger().MessageLogException(e);
					}
				}
				dispatching = false;
			}
		}
	}
}