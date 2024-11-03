using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class) WebSocket Server<br/>
    /// Class to execute a WebSocket Server on local.
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.04 - July 04, 2024
    /// </remarks>
    public class WebSocketServer : WebSocketServerUtils 
    {

        #region Properties

        private HttpListener? _httpServerListener = null;
        private List<WebSocketServerClient>? _clientsConnected = null;
        private HashSet<Task> _requestThreads = null;
        private Dictionary<string, WebSocketServerCommand> _CommandList = new Dictionary<string, WebSocketServerCommand>();
        private Dictionary<string, WebSocketServerCommand<Task>> _CommandListAsync = new Dictionary<string, WebSocketServerCommand<Task>>();

        public int port { get; set; } = 8081;
        public int maxConnections { get; set; } = 10;
        public string apiKey { get; set; } = "";
        public List<string>? endpoints { get; set; } = null;
        public bool isRunning { get; set; } = false;
        public CancellationTokenSource? cToken { get; set; } = null;
        public event EventHandler<WebSocketServerEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a WebSocket Server
        /// </summary>
        /// <param name="port">WebSocket Server Port (Default: 8080)</param>
        /// <param name="maxConnections">Max Number of Connections</param>
        ///  <param name="apiKey">API Key to allow requests</param>
        /// <param name="autostart">Start sever on Creation</param>
        public WebSocketServer(int port = 8080, int maxConnections = 10, string apiKey = "",  bool autostart = false)
        {
            this.port = port;
            this.maxConnections = maxConnections;
            this.apiKey = apiKey;
            Logger.Add(LogLevel.OnlyDebug, "WebSocket Server", $"Server created. Port: <{port}> | Max Connections: <{maxConnections}>.");

            // Add Default Actions (Test)
            AddCommand("hello", (client, payload) => _ = SendToClient(client, new WebSocketServerMsgSent("hello", "hello world! how are you?").ToString(), CancellationToken.None));
            AddCommand("echo", (client, payload) => _ = SendToClient(client, new WebSocketServerMsgSent("echo", payload).ToString(), CancellationToken.None));
            AddCommand("echoAll", (client, payload) => _ = SendToAllClients(this._clientsConnected, new WebSocketServerMsgSent("echoAll", payload).ToString(), CancellationToken.None));
            AddCommand("users", (client, payload) => { _ = SendToClient(client, new WebSocketServerMsgSent("users", this._clientsConnected).ToString(), CancellationToken.None); });

            if (autostart)
                this.Start();
        }

        /// <summary>
        /// Update settings of WebSocket Server
        /// </summary>
        /// <param name="port">WebSocket Server Port (Default: 8080)</param>
        /// <param name="maxConnections">Max Number of Connections</param>
        ///  <param name="apiKey">API Key to allow requests</param>
        /// <param name="restart">Restart Server on Update</param>
        /// <returns>(bool *async)</returns>
        public async Task Update(int port = 8080, int maxConnections = 10, string apiKey = "", bool restart = true)
        {
            if (restart)
                this.Stop();
            this.port = port;
            this.maxConnections = maxConnections;
            this.apiKey = apiKey;
            if (restart)
                this.Start();
            Logger.Add(LogLevel.Info, "WebSocket Server", $"Updated Settings. Port: <{port}> | Max Connections: <{maxConnections}>.");
        }

        #endregion

        #region Start / Stop

        /// <summary>
        /// Start the WebSocket Server
        /// </summary>
        /// <returns>(void *async)</returns>
        public async Task Start()
        {
            // If running cancel start and return.
            if (this.isRunning)
            {
                Logger.Add(LogLevel.Info, "WebSocket Server", $"Server already running.");
                return;
            }

            // Get End Points
            this.endpoints = Utils.GetEndPointsWithPort(this.port, true, false, false, true);

            // Force the use of cancellation Token
            if (this.cToken == null)
            {
                this.cToken = new CancellationTokenSource();
                Logger.Add(LogLevel.Warning, "WebSocket Server", $"No Cancellation Token used. Token creation forced.");
            }

            // Start Server, handle clients and all listening endpoints
            _httpServerListener = new HttpListener();
            foreach (String endpoint in this.endpoints)
            {
                _httpServerListener.Prefixes.Add(endpoint);
                Logger.Add(LogLevel.Success, "WebSocket Server", $"Listening connections on <{endpoint}>.");
            }
            _httpServerListener.Start();
            NotifyChange("started", null);

            // Manage multiple requests tasks (threads)
            _requestThreads = new HashSet<Task>();
            _requestThreads.Clear();
            for (int i = 0; i < this.maxConnections+1; i++)
                _requestThreads.Add(_httpServerListener.GetContextAsync());

            // Handle Requests (Continous Loop)
            while (!this.cToken.IsCancellationRequested)
            {
                NotifyChange("running", null);

                // Listen for requests in all the threads and remove that thread from threads available
                Task _singleRequestThread = await Task.WhenAny(_requestThreads);
                _requestThreads.Remove(_singleRequestThread);

                // Get the HTTP Listener Context
                HttpListenerContext context = (_singleRequestThread as Task<HttpListenerContext>).Result;
                WebSocketServerClient? client = await RequestContextAsync(context);

                // Limit of Max Connections is reached, reject connection and add again a request thread
                if (_requestThreads.Count == 0)
                {
                    if (client != null)
                        await ForceDisconnectionResponseAsync(client, this.cToken);
                    continue;
                }

                // Process request
                if (_singleRequestThread is Task<HttpListenerContext>)
                {
                    _ = Task.Run(async() => {
                        // Wait for a Message from Client And Serve a Response
                        if (client != null)
                            await WaitMessageAndServeResponseAsync(client, this.cToken);
                        // Add again a request thread after serve response
                        _requestThreads.Add(_httpServerListener.GetContextAsync());
                        Logger.Add(LogLevel.Info, "WebSocket Server", $"User disconnected. Total users: (#{_clientsConnected?.Count}). Threads Remaining: ({_requestThreads.Count - 1}).");
                    }, this.cToken.Token);
                }
                else
                {
                    // Add again a request thread 
                    _requestThreads.Add(_httpServerListener.GetContextAsync());
                    Logger.Add(LogLevel.Fatal, "WebSocket Server", $"HTTP Context not found.");
                }
            }

            // On Cancellation
            _requestThreads.Clear();

            // Dispose Token
            this.cToken = null;
        }

        /// <summary>
        /// Handle HTTP Request on WebSocket Server
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns>(void *async)</returns>
        public async Task<WebSocketServerClient?> RequestContextAsync(HttpListenerContext context)
        {
            // Peel out the requests and response objects
            HttpListenerRequest? request = context.Request;
            HttpListenerResponse? response = context.Response;

            // If Request is not a WebSocket Request Close Connection
            if (!request.IsWebSocketRequest)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
                NotifyChange("badRequest", null);
                Logger.Add(LogLevel.Error, "WebSocket Server", $"Bad Request: User tried to connect.");
                return null;
            }

            // Get WebSocket Context
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            // Init Clients Connected 
            if (_clientsConnected == null)
                _clientsConnected = new List<WebSocketServerClient>();

            // Create New User Client
            WebSocketServerClient client = new WebSocketServerClient(request, webSocket);
            _clientsConnected.Add(client);

            NotifyChange("userConnected", client);
            Logger.Add(LogLevel.Info, "WebSocket Server", $"User connected. Total users: (#{_clientsConnected.Count}). Threads Remaining: ({_requestThreads.Count}).", client.UIDshort);
            return client;
        }

        /// <summary>
        /// Wait listening until message is received and serve response / execute a callback Async
        /// </summary>
        /// <param name="client">Web Socket Server Client (User)</param>
        /// <param name="cToken">Cancellation Token</param>
        /// <returns>(void *async)</returns>
        public async Task WaitMessageAndServeResponseAsync(WebSocketServerClient client, CancellationTokenSource cToken)
        {
            // Welcome message
            _ = SendToClient(client, new WebSocketServerMsgSent("welcome", $"Hi. Welcome {client.IPAddress} <{client.UIDshort}>! Connected On: {client.connectedOn}").ToString(), cToken.Token);

            // Create buffer for result
            var buffer = new byte[10240];
            WebSocketReceiveResult result;

            while ((client.WebSocket.State == WebSocketState.Open) && (!cToken.IsCancellationRequested))
            {
                // Wait until a Message from client is Received
                result = await client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cToken.Token);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                NotifyChange("messageReceived", client);

                // Close Request
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cToken.Token);
                    _clientsConnected?.Remove(client);
                    NotifyChange("userDisconnected", client);
                }
                // Message Request
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Parse Mesasge Received
                    WebSocketServerMsgReceived msgReceived = WebSocketServerMsgReceived.ConvertTo(client, message);

                    // Impossible to parse
                    if (msgReceived == null)
                    {
                        _ = SendToClient(client, new WebSocketServerMsgSent("error", $"JSON Incorrect format.").ToString(), cToken.Token);
                        continue;
                    }
                    // Wrong API Key
                    if (!msgReceived.apiKey.Equals(this.apiKey))
                    {
                        _ = SendToClient(client, new WebSocketServerMsgSent("badApiKey", $"Incorrect API KEY, impossible to proceed.").ToString(), cToken.Token);
                        Logger.Add(LogLevel.Warning, "WebSocket Server", $"Incorrect API KEY, impossible to proceed.", client.UIDshort);
                        continue;
                    }
                    // Search action on command list
                    if (_CommandList.Keys.Contains(msgReceived.action))
                    {
                        _CommandList.TryGetValue(msgReceived.action, out WebSocketServerCommand command);
                        command.callback(client, msgReceived.payload);
                    }
                    // Search action on command list async
                    else if (_CommandListAsync.Keys.Contains(msgReceived.action))
                    {
                        _CommandListAsync.TryGetValue(msgReceived.action, out WebSocketServerCommand<Task> command);
                        await command.callback(client, msgReceived.payload);
                    }
                    // Command not found
                    else
                    {
                        _ = SendToClient(client, new WebSocketServerMsgSent("error", $"Action ({msgReceived.action}) doesn't have any defined task.").ToString(), cToken.Token);
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Send an error message to client and force disconnection
        /// </summary>
        /// <param name="client">Web Socket Server Client (User)</param>
        /// <param name="cToken">Cancellation Token</param>
        /// <returns>(void *async)</returns>
        public async Task ForceDisconnectionResponseAsync(WebSocketServerClient client, CancellationTokenSource cToken)
        {
            // Send Error to Client
            _ = SendToClient(client, new WebSocketServerMsgSent("error", $"Limit of Max Connections reached. Connection Rejected.").ToString(), cToken.Token);

            // Disconnect Server
            await client.WebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, string.Empty, cToken.Token);

            // Remove from Clients Connected
            _clientsConnected?.Remove(client);

            // Add again a request thread after serve response
            _requestThreads.Add(_httpServerListener.GetContextAsync());
            NotifyChange("userDisconnectedForced", client);
            Logger.Add(LogLevel.Info, "WebSocket Server", $"User disconnected (forced). Total users: (#{_clientsConnected?.Count}). Threads Remaining: ({_requestThreads.Count}).", client.UIDshort);
            Logger.Add(LogLevel.Error, "WebSocket Server", $"Limit of Max Connections reached. Connection Rejected.");
        }

        /// <summary>
        /// Stop the Web Socket Server
        /// </summary>
        /// <returns>(void)</returns>
        public void Stop()
        {
            if (!this.isRunning || _httpServerListener == null)
            {
                Logger.Add(LogLevel.Info, "WebSocket Server", $"Server not running.");
                return;
            }
            try
            {
                if (cToken != null)
                    cToken.Cancel();
                _httpServerListener?.Stop();
                _httpServerListener?.Close();
                _clientsConnected?.Clear();
                _requestThreads?.Clear();
                cToken = null;
                Logger.Add(LogLevel.Info, "WebSocket Server", $"Server Stopped and Closed.");
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "WebSocket Server", $"Error stopping Web Socket Server", $"Exception: {ex.Message}.");
            }
            NotifyChange("stopped", null);
        }

        #endregion

        #region Add Commands of WebSocket Server

        /// <summary>Add a *Command*, defining an action to be triggered on request.<br/>
        /// <example>
        /// Example:<br/>
        /// **Defining Command**<br/>
        /// AddCommand("COMMAND", FunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public void FunctionToExecute(dynamic payload)<br/>{<br/>return;<br/>}
        /// </example>
        /// </summary>
        /// <param name="command">Requested Path</param>
        /// <param name="callback">Function name to be called. (Note: Function should have *string* as input variable (payload). Return should be of type void.)</param>
        /// <returns>(void)</returns>
        public void AddCommand(string command, Action<WebSocketServerClient, dynamic>? callback = null)
        {
            WebSocketServerCommand response;
            response.callback = callback;
            if (!_CommandList.ContainsKey(command))
                _CommandList.Add(command, response);
            else
                Logger.Add(LogLevel.Warning, "WebSocket Server", $"Command duplicated: <{command}>.");
        }

        /// <summary>Add a *Command*, defining an (async) action to be triggered on request.<br/>
        /// <example>
        /// Example:<br/>
        /// **Defining Command**<br/>
        /// AddCommandAsync("COMMAND", AsyncFunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public Task AsyncFunctionToExecute(dynamic payload)<br/>{<br/>return;<br/>}
        /// </example>
        /// </summary>
        /// <param name="command">Requested Path</param>
        /// <param name="callback">Function name to be called. (Note: Async Function should have *string* as input variable (payload). Return should be of type void.)</param>
        /// <returns>(void)</returns>
        public void AddCommandAsync(string command, Func<WebSocketServerClient, dynamic, Task>? callback = null)
        {
            WebSocketServerCommand<Task> response;
            response.callback = callback;
            if (!_CommandListAsync.ContainsKey(command))
                _CommandListAsync.Add(command, response);
            else
                Logger.Add(LogLevel.Warning, "WebSocket Server", $"Command Async duplicated: <{command}>.");
        }

        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on WebSocket Server.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType, WebSocketServerClient client)
        {
            this.isRunning = (_httpServerListener != null) ? _httpServerListener.IsListening : false;
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new WebSocketServerEvent(eventType, client));
            }
        }

        #endregion
    }
}