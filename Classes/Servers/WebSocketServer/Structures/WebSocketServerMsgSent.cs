using System.Text.Json;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class/Object Definition) WebSocket Server Message Sent
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.18 - June 18, 2024
    /// </remarks>
    public class WebSocketServerMsgSent
    {
        public string result { get; set; } = String.Empty;
        public dynamic payload { get; set; } = null;

        /// <summary>
        /// Represent a Message Sent to Client
        /// </summary>
        /// <param name="result">Alias for command result</param>
        /// /// <param name="payload">Payload / Data</param>
        public WebSocketServerMsgSent(string result = "", dynamic payload = null)
        {
            this.result = result;
            this.payload = payload;
        }

        /// <summary>
        /// Convert to objet to string (override)
        /// </summary>
        public override string ToString() {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}