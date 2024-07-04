using System.Net;
using System.Net.WebSockets;
using System.Text.Json.Serialization;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class/Object Definition) WebSocket Server Client (User)
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.18 - June 18, 2024
    /// </remarks>
    public class WebSocketServerClient
    {
        public Guid UID { get; set; }
        public DateTime connectedOn { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string OperativeSystem { get; set; } = string.Empty;
        public string UIDshort { get => this.UID.ToString().Substring(0, 8); }
        [JsonIgnore]
        public WebSocket WebSocket { get; set; }

        /// <summary>
        /// Represent the connection and data of user connected to the server
        /// </summary>
        /// <param name="request">HTTP Context Request</param>
        /// <param name="webSocket">Web Socket Reference</param>
        public WebSocketServerClient(HttpListenerRequest request, WebSocket webSocket)
        {
            this.UID = Guid.NewGuid();
            this.WebSocket = webSocket;
            this.connectedOn = DateTime.Now;
            this.IPAddress = ((IPEndPoint)request.RemoteEndPoint).Address.ToString();
            this.UserAgent = request.UserAgent;
            this.OperativeSystem = Utils.GetOSFromUserAgent(request.UserAgent);
        }
    }
}