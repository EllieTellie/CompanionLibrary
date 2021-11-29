namespace CompanionFramework.Core.Log
{
	public enum LogLevel
	{
		/// <summary>
		/// Logs everything.
		/// </summary>
		All,
		/// <summary>
		/// Logs Debug, Message, Warning, Error and Exceptions.
		/// </summary>
		Debug,
		/// <summary>
		/// Logs Message, Warning, Error and Exceptions.
		/// </summary>
		Message,
		/// <summary>
		/// Logs Warning, Error and Exceptions.
		/// </summary>
		Warning,
		/// <summary>
		/// Logs Error and Exceptions.
		/// </summary>
		Error,
		/// <summary>
		/// Logs Exceptions.
		/// </summary>
		Exception,
		/// <summary>
		/// Logs nothing.
		/// </summary>
		None
	}
}
