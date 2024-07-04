namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class/Object Definition) Light HTTP Server Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.17 - June 17, 2024
    /// </remarks>
    public class LightHTTPServerEvent : EventArgs
    {
        public string? eventType { get; } = null;

        /// <summary>
        /// Event on Light HTTP Server
        /// </summary>
        /// <param name="eventType">Event Type</param>
        public LightHTTPServerEvent(string eventType)
        {
            this.eventType = eventType;
        }
    }
}