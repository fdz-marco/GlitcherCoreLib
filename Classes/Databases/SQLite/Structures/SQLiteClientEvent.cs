﻿namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class/Object Definition) Light SQLite Client Event (EventArgs)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.18 - July 18, 2024
    /// </remarks>

    public class SQLiteClientEvent : EventArgs
    {
        public string? eventType { get; } = null;

        /// <summary>
        /// Event on Light SQLite Client
        /// </summary>
        /// <param name="eventType">Event Type</param>
        public SQLiteClientEvent(string eventType)
        {
            this.eventType = eventType;
        }
    }
}