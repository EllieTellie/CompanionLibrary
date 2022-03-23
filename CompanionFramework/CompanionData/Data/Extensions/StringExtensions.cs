namespace Companion.Data
{
	public static class StringExtensions
	{
		/// <summary>
		/// Trim all the whitespace from the strings in this array.
		/// </summary>
		/// <param name="arr">Array</param>
		public static void Trim(this string[] arr)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				string s = arr[i];
				arr[i] = s.Trim();
			}
		}

		/// <summary>
		/// Trim all the leading whitespace from the strings in this array.
		/// </summary>
		/// <param name="arr">Array</param>
		public static void TrimStart(this string[] arr)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				string s = arr[i];
				arr[i] = s.TrimStart();
			}
		}
	}
}
