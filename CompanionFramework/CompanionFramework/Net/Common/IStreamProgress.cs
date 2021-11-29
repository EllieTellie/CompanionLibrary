namespace CompanionFramework.Net.Http.Common
{
	/// <summary>
	/// Update stream reading progress.
	/// </summary>
	public interface IStreamProgress
	{
		/// <summary>
		/// Update progress of the stream reading.
		/// </summary>
		/// <param name="bytesReceived">Bytes received</param>
		/// <param name="totalBytes">Total bytes</param>
		void UpdateProgress(long bytesReceived, long totalBytes);
	}
}
