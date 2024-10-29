namespace glitcher.core.Clients
{
    /// <summary>
    /// (Class/Object Definition) MQTT Client Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.10.25 - October 25, 2024
    /// </remarks>
    public class MQTTClientEvent : EventArgs
    {
        public string? eventType { get; } = null;

        /// <summary>
        /// Event on MQTT Client
        /// </summary>
        /// <param name="eventType">Event Type</param>
        public MQTTClientEvent(string eventType)
        {
            this.eventType = eventType;
        }
    }
}