namespace glitcher.core
{
    /// <summary>
    /// (Class/Object Definition) Log (Entry)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.17 - June 17, 2024
    /// </remarks>
    public class LogEntry
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        public LogLevel LogLevel { get; set; } = LogLevel.None;
        public string Group { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Whisper { get; set; } = string.Empty;
        public string Caller { get; set; } = string.Empty;
    }

}