namespace glitcher.core.PLC.Beckhoff
{
    /// <summary>
    /// (Class/Object Definition) ADS Client Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.04.26 - April 04, 2024
    /// </remarks>

    public class ADSClientEvent : EventArgs
    {
        public string? eventType { get; } = null;

        /// <summary>
        /// Event on ADS Client
        /// </summary>
        /// <param name="eventType">Event Type</param>
        public ADSClientEvent(string eventType)
        {
            this.eventType = eventType;
        }
    }
}