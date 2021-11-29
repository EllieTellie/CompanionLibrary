using System;

namespace CompanionFramework.Core.Threading.Messaging
{
	/// <summary>
	/// Interface for dispatching messages on the event queue.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMessageDispatcher<T>
				where T : EventArgs
	{
		/// <summary>
		/// Fire this message on the dispatcher.
		/// </summary>
		/// <param name="source">Event Source</param>
		/// <param name="eventArgs">Event Arguments</param>
		void Dispatch(Object source, T eventArgs);
	}
}
