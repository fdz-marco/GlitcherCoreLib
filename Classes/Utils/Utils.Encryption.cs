namespace glitcher.core
{
    /// <summary>
    /// (Class: Static~Global) **Utilities - Encryption**<br/><br/>
    /// </summary>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.22 - July 22, 2024
    /// </remarks>
    public static partial class Utils
	{
		/// <summary>
		/// Encode to Base64
		/// </summary>
		/// <param name="plainText">Text to Encode</param>
		/// <returns>(string) Text encoded</returns>
		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		/// <summary>
		/// Decode from Base64
		/// </summary>
		/// <param name="base64EncodedData">Text to Decode</param>
		/// <returns>(string) Text decoded</returns>
		public static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}

	}
}
