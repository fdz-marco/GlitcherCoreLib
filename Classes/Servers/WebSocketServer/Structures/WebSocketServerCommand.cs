namespace glitcher.core.Servers
{
    /// <summary>
    /// (Structure) WebSocket Server Command
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.18 - June 18, 2024
    /// </remarks>
    public struct WebSocketServerCommand<T>
    {
        public Func<WebSocketServerClient, dynamic, T> callback;
    }

    public struct WebSocketServerCommand
    {
        public Action<WebSocketServerClient, dynamic> callback;
    }
}