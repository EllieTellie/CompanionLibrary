namespace Companion.Data.System.Update
{
	public enum UpdateError
	{
		/// <summary>
		/// Default generic error
		/// </summary>
		Error,
		/// <summary>
		/// No state was not provided to the process (null).
		/// </summary>
		MissingState,
		/// <summary>
		/// Invalid parameter passed to the process.
		/// </summary>
		InvalidParameter,
		/// <summary>
		/// We failed to get a valid network response
		/// </summary>
		FailedNetworkResponse,
		/// <summary>
		/// Repository index data from the network was invalid
		/// </summary>
		InvalidRepositoryIndex,
		/// <summary>
		/// Unable to access files.
		/// </summary>
		FailedFileAccess
	}
}
