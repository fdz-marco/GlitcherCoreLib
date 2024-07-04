namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class/Object Definition) WebSocket Server Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.02 - July 02, 2024
    /// </remarks>
    public class WebSocketServerEvent : EventArgs
    {
        public string? eventType { get; } = null;

        public WebSocketServerClient? client { get; } = null;

        /// <summary>
        /// Event on WebSocket Server
        /// </summary>
        /// <param name="eventType">Event Type</param>
        /// <param name="client">Client</param>
        public WebSocketServerEvent(string eventType, WebSocketServerClient client)
        {
            this.eventType = eventType;
            this.client = client;
        }
    }
}