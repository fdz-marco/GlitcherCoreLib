namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class/Object Definition) Light MySQL Client Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.18 - July 18, 2024
    /// </remarks>

    public class MySQLClientEvent : EventArgs
    {
        public string? eventType { get; } = null;

        /// <summary>
        /// Event on Light MySQL Client
        /// </summary>
        /// <param name="eventType">Event Type</param>
        public MySQLClientEvent(string eventType)
        {
            this.eventType = eventType;
        }
    }
}