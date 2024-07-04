using System.Text;
using System.Net.WebSockets;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class) Web Socket Server - Utilities<br/>
    /// Support / Utilities Functions for Web Socket Server
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.18 - June 18, 2024
    /// </remarks>
    public class WebSocketServerUtils
    {

        /// <summary>
        /// Send a message to client
        /// </summary>
        /// <param name="client">Web Socket Server Client (User)</param>
        /// <param name="message">Message to send to client</param>
        /// <param name="cToken">Cancellation Token</param>
        public async Task<bool> SendToClient(WebSocketServerClient client, string message, CancellationToken cToken)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await client.WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cToken);
                Logger.Add(LogLevel.Success, "WebSocket Server", $"Message sent to client.", client.UIDshort);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "WebSocket Server", $"Message not sent to client. Error: {ex.Message}.", client.UIDshort);
                return false;
            }
        }

        /// <summary>
        /// Send a binary to client
        /// </summary>
        /// <param name="client">Web Socket Server Client (User)</param>
        /// <param name="stream">Binary to send to client</param>
        /// <param name="cToken">Cancellation Token</param>
        protected async Task<bool> SendToClient(WebSocketServerClient client, Stream stream, CancellationToken cToken)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] buffer = ms.ToArray();
                await client.WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cToken);
                Logger.Add(LogLevel.Success, "WebSocket Server", $"Message sent to client.", client.UIDshort);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "WebSocket Server", $"Message not sent to client. Error: {ex.Message}.", client.UIDshort);
                return false;
            }

        }

        /// <summary>
        /// Send a message to all clients connected
        /// </summary>
        /// <param name="connectedClients">List of Web Socket Server Clients (Users)</param>
        /// <param name="message">Message to send to client</param>
        /// <param name="cToken">Cancellation Token</param>
        protected async Task SendToAllClients(List<WebSocketServerClient> connectedClients, string message, CancellationToken cToken)
        {
            foreach (WebSocketServerClient client in connectedClients)
            {
                await SendToClient(client, message, cToken);
                if (cToken.IsCancellationRequested) break;
            }
        }

        /// <summary>
        /// Send a binary to all clients connected
        /// </summary>
        /// <param name="connectedClients">List of Web Socket Server Clients (Users)</param>
        /// <param name="stream">Binary to send to client</param>
        /// <param name="cToken">Cancellation Token</param>
        protected async Task SendToAllClients(List<WebSocketServerClient> connectedClients, Stream stream, CancellationToken cToken)
        {
            foreach (WebSocketServerClient client in connectedClients)
            {
                await SendToClient(client, stream, cToken);
                if (cToken.IsCancellationRequested) break;
            }
        }
    }
}