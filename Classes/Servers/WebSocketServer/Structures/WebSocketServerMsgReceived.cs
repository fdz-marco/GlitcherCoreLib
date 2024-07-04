using System.Text.Json;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class/Object Definition) WebSocket Server Message Received
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.02 - July 02, 2024
    /// </remarks>
    public class WebSocketServerMsgReceived
    {
        public string apiKey { get; set; } = String.Empty;
        public string action { get; set; } = String.Empty;
        public dynamic payload { get; set; } = null;

        /// <summary>
        /// Represent a Message Received from Client
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <param name="action">Alias for trigger a command</param>
        /// /// <param name="payload">Payload / Data</param>
        public WebSocketServerMsgReceived(string apiKey = "", string action = "", dynamic payload = null)
        {
            this.apiKey = apiKey;
            this.action = action;
            this.payload = payload;
        }

        /// <summary>
        /// Convert to objet to string (override)
        /// </summary>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Convert to WebSocket Server Message Received
        /// </summary>
        /// <param name="client">WebSocket Server Client (User)</param>
        /// <param name="message">Message Received from Client</param>
        public static WebSocketServerMsgReceived? ConvertTo(WebSocketServerClient client, string message)
        {
            string apiKey = String.Empty;
            string action = String.Empty;
            dynamic payload = null;
            try
            {
                JsonDocument document = JsonDocument.Parse(message);
                JsonElement root = document.RootElement;
                apiKey = root.GetProperty("apiKey").GetString();
                action = root.GetProperty("action").GetString();
                payload = root.GetProperty("payload").GetRawText();
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "WebSocket Server", $"Impossible to parse incoming message. Error: {ex.Message}", client.UIDshort);
                return null;
            }

            return new WebSocketServerMsgReceived(apiKey, action, payload);
        }
    }
}