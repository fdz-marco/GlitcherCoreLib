namespace glitcher.core
{
    /// <summary>
    /// (Class/Object Definition) Log Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.17 - June 17, 2024
    /// </remarks>
    public class LogEvent : EventArgs
    {
        public string? eventType { get; } = null;
        public dynamic? variable { get; } = null;

        /// <summary>
        /// Event on Log
        /// </summary>
        /// <param name="eventType">Event Type</param>
        /// <param name="variable">Variable to Notify</param>
        public LogEvent(string eventType, dynamic? variable = null)
        {
            this.eventType = eventType;
            this.variable = variable;
        }

        // ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
    }
}