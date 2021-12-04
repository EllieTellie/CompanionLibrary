
namespace CompanionFramework.Core.Threading.Messaging
{
	/// <summary>
	/// Wrapper class for the message queue. Accessed by lazy initialized singleton.
	/// </summary>
	public class MessageHandler
	{
		public const float defaultMessageQueueDelay = 0.05f;
		public const int defaultMessageQueueDelayInt = 50;

		private MessageQueue messageQueue;

		private static MessageHandler instance;

		/// <summary>
		/// Access the message handler singleton. If it's not set it will create a new message handler.
		/// </summary>
		/// <returns>Message handler singleton</returns>
		public static MessageHandler Instance()
		{
			if (instance == null)
				instance = new MessageHandler();

			return instance;
		}

		/// <summary>
		/// Create a new message handler with a message queue.
		/// </summary>
		public MessageHandler()
		{
			this.messageQueue = new MessageQueue();
		}

		/// <summary>
		/// Get the message queue.
		/// </summary>
		/// <returns>Returns the message queue</returns>
		public MessageQueue GetMessageQueue()
		{
			return messageQueue;
		}

		/// <summary>
		/// Fires any messages in the message queue. Should be called from a main loop or timer loop.
		/// </summary>
		public void ProcessQueue()
		{
			messageQueue.Dispatch();
		}

		/// <summary>
		/// Static way to detect if the message handler is used.
		/// </summary>
		/// <returns>True if using the message handler</returns>
		public static bool HasMessageHandler()
		{
			return instance != null;
		}
	}
}