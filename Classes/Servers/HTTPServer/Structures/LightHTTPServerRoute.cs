using System.Collections.Specialized;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Structure) Light HTTP Server Route
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.17 - June 17, 2024
    /// </remarks>
    public struct LightHTTPServerRoute<T>
    {
        public Func<NameValueCollection, T> callback;
        public string mimeType;
    }
}