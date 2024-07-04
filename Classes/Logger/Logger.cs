namespace glitcher.core
{
    /// <summary>
    /// (Class: Static~Global) Simple Logger<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.17 - June 17, 2024
    /// </remarks>
    public static class Logger
    {
        #region Properties

        public static List<LogEntry> logList { get; set; } = new List<LogEntry>();
        public static bool debug { get; set; } = true;
        public static bool disabled { get; set; } = false;
        public static bool includeCaller { get; set; } = true;
        public static event EventHandler<LogEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Logger Events
        /// </summary>
        static Logger()
        {
            if (logList == null)
            {
                logList = new List<LogEntry>();
            }
        }

        #endregion

        #region Log Actions

        /// <summary> 
        /// Add Entry to Log.<br/>
        /// <example>
        /// Example:<br/>
        /// <code>Logger.Add(LogLevel.Error, "Class Name", "Error on Step 1", "More Info);</code>
        /// </example>
        /// </summary>
        /// <param name="level">Severity Level</param>
        /// <param name="group">Group</param>
        /// <param name="message">Message</param>
        /// <param name="whisper">Whisper (More information)</param>
        /// <returns>(void)</returns>
        public static void Add(LogLevel level, string group, string message, string whisper = "")
        {
            if (disabled) 
                return;

            LogEntry log = new LogEntry();
            log.DateTime = DateTime.Now;
            log.LogLevel = level; 
            log.Group = group;
            log.Message = message;
            log.Whisper = whisper;

            var stackFrame = new System.Diagnostics.StackTrace().GetFrame(1);
            var methodInfo = (stackFrame != null) ? stackFrame.GetMethod() : null; 
            var className = (methodInfo?.ReflectedType != null) ? methodInfo.ReflectedType.FullName : null;
            log.Caller = (className != null) ? className : "";
            
            string debugMsg = $"[{log.DateTime}][{log.LogLevel}][{log.Group}] {log.Message}";
            debugMsg += string.IsNullOrEmpty(log.Whisper) ? string.Empty : $" ({log.Whisper})";
            debugMsg += includeCaller ? $" <{log.Caller}>" : string.Empty;

            _d(debugMsg);
            logList.Add(log);
            NotifyChange("add", log);
        }

        /// <summary>
        /// Get a JSON of all logs
        /// </summary>
        /// <returns>(string) JSON with alll the logs.</returns>
        public static string GetAllJSON()
        {
            return System.Text.Json.JsonSerializer.Serialize(logList);
        }

        /// <summary>
        /// Clear all logs
        /// </summary>
        /// <returns>(void)</returns>
        public static void ClearAll()
        {
            logList.Clear();
            NotifyChange("clear");
        }

        /// <summary>
        /// Alias to write a message on console
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <returns>(void)</returns>
        public static void _d(string message)
        {
            if (debug)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }
        }

        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on Log
        /// </summary>
        /// <param name="eventType">Event Type</param>
        /// <param name="variable">Variable to Notify</param>
        /// <returns>(void)</returns>
        private static void NotifyChange(string eventType, dynamic? variable = null)
        {
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(typeof(Logger), new LogEvent(eventType, variable));
            }
        }

        #endregion

    }
}