using System.Net;
using System.Collections.Specialized;
using System.Reflection;

namespace glitcher.core.Servers
{
    /// <summary>
    /// (Class) Light HTTP Server <br/>
    /// Class to execute a HTTP Server on local.<br/>
    /// The class allows to serve local files, embededd resources, and custom responses.<br/><br/>
    /// **Important**<br/>
    /// To add complete project folder as embedded resources, 
    /// please add the folder as embedded resource in the *.csproj definition:<br/>
    /// <code>&lt;ItemGroup&gt; &lt;EmbeddedResource Include = "Html\*"&gt; &lt;ItemGroup&gt;</code>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.04 - July 04, 2024
    /// </remarks>
    public class LightHTTPServer : LightHTTPServerUtils
    {

        #region Properties

        private HttpListener? _httpServerListener = null;
        private HashSet<Task> _requestThreads = null;
        private Dictionary<string, LightHTTPServerRoute<string>> _RoutesList = new Dictionary<string, LightHTTPServerRoute<string>>();
        private Dictionary<string, LightHTTPServerRoute<Task<string>>> _RoutesListAsync = new Dictionary<string, LightHTTPServerRoute<Task<string>>>();

        public int port { get; set; } = 8080;
        public int maxConnections { get; set; } = 10;
        public List<string>? endpoints { get; set; } = null;
        public string basePathLocal { get; set; } = "www";        
        public string basePathEmbedded { get; set; } = "Html";
        public bool allowCrossOrigin { get; set; } = false;
        public bool isRunning { get; set; } = false;      
        public bool ServeFirstLocal { get; set; } = false;
        public CancellationTokenSource? cToken { get; set; } = null;
        public event EventHandler<LightHTTPServerEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a Light HTTP Server
        /// </summary>
        /// <param name="port">HTTP Server Port (Default: 8080)</param>
        /// <param name="maxConnections">Max Number of Connections</param>
        /// <param name="basePathEmbedded">The base path to find the embedded resource files (Note: Relative to root folder of project).</param>
        /// <param name="basePathLocal">The base path to find the local files (Note: Relative to Application directory).</param>
        /// <param name="allowCrossOrigin">Allow Cross Origin (CORS)</param>
        /// <param name="autostart">Start sever on creation</param>
        public LightHTTPServer(int port = 8080, int maxConnections = 10, string basePathEmbedded = "Html", string basePathLocal = "www", bool allowCrossOrigin = false, bool autostart = false)
        {
            this.port = port;
            this.maxConnections = maxConnections;
            this.basePathEmbedded = basePathEmbedded;
            this.basePathLocal = basePathLocal;
            this.allowCrossOrigin = allowCrossOrigin;
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"Server created. Port: <{port}> | Max Connections: <{maxConnections}> | Base Path Embeded: <{basePathEmbedded}> | Base Path Local: <{basePathLocal}> | Allow CrossOrigin: <{allowCrossOrigin}>.");
            if (autostart)
                this.Start();
        }

        /// <summary>
        /// Update settings of HTTP Server
        /// </summary>
        /// <param name="port">HTTP Server Port (Default: 8080)</param>
        /// <param name="maxConnections">Max Number of Connections</param>
        /// <param name="basePathLocal">The base path to find the local files (Note: Relative to Application directory).</param>
        /// <param name="allowCrossOrigin">Allow Cross Origin (CORS)</param>
        /// <param name="restart">Restart Server on Update</param>
        /// <returns>(bool *async)</returns>
        public async Task Update(int port = 8080, int maxConnections = 10, string basePathLocal = "www", bool allowCrossOrigin = false, bool restart = true)
        {
            if (restart)
                this.Stop();
            this.cToken = cToken;
            this.port = port;
            this.maxConnections = maxConnections;
            this.basePathLocal = basePathLocal;
            this.allowCrossOrigin = allowCrossOrigin;
            if (restart)
                this.Start();
            Logger.Add(LogLevel.Info, "HTTP Server", $"Updated Settings. Port: <{port}> | Max Connections: <{maxConnections}> | Base Path Embeded: <{basePathEmbedded}> | Base Path Local: <{basePathLocal}> | Allow CrossOrigin: <{allowCrossOrigin}>.");
        }

        #endregion

        #region Start / Stop

        /// <summary>
        /// Start the HTTP Server.
        /// </summary>
        /// <returns>(void *async)</returns>
        public async Task Start()
        {
            // If running cancel start and return.
            if (this.isRunning)
            {
                Logger.Add(LogLevel.Info, "HTTP Server", $"Server already running.");
                return;
            }

            // Get End Points
            this.endpoints = Utils.GetEndPointsWithPort(this.port, true, false, false, true);

            // Force the use of cancellation Token
            if (this.cToken == null)
            {
                this.cToken = new CancellationTokenSource();
                Logger.Add(LogLevel.Warning, "HTTP Server", $"No Cancellation Token used. Token creation forced.");
            }

            // Start Server and all listening endpoints
            _httpServerListener = new HttpListener();
            foreach (String endpoint in this.endpoints)
            {
                _httpServerListener.Prefixes.Add(endpoint);
                Logger.Add(LogLevel.Success, "HTTP Server", $"Listening connections on <{endpoint}>.");
            }
            _httpServerListener.Start();
            NotifyChange("started");

            // Manage multiple requests tasks (threads)
            _requestThreads = new HashSet<Task>();
            _requestThreads.Clear();
            for (int i = 0; i < this.maxConnections+1; i++)
                _requestThreads.Add(_httpServerListener.GetContextAsync());

            // Handle Requests (Continous Loop)
            while (!this.cToken.IsCancellationRequested)
            {
                NotifyChange("running");

                // Listen for requests in all the threads and remove that thread from threads available
                Task _singleRequestThread = await Task.WhenAny(_requestThreads);
                _requestThreads.Remove(_singleRequestThread);

                // Limit of Max Connections is reached, reject connection and add again a request thread
                if (_requestThreads.Count == 0)
                {
                    Logger.Add(LogLevel.Fatal, "HTTP Server", "No HTTP Server Threads available.");
                    // Add again a request thread after deny response
                    _requestThreads.Add(_httpServerListener.GetContextAsync());
                    continue;
                }

                // Process request
                if (_singleRequestThread is Task<HttpListenerContext>)
                {
                    _ = Task.Run(async() => {
                        // Get Task Result (shuold be HTTP Listener Context) and process it
                        HttpListenerContext context = (_singleRequestThread as Task<HttpListenerContext>).Result;
                        if (context != null)
                            await RequestContextAsync(context);
                        // Add again a request thread after serve response
                        _requestThreads.Add(_httpServerListener.GetContextAsync());
                        Logger.Add(LogLevel.Info, "HTTP Server", $"Content served. Threads Remaining: ({_requestThreads.Count - 1}).");
                    }, this.cToken.Token);
                }
                else
                {
                    // Add again a request thread 
                    _requestThreads.Add(_httpServerListener.GetContextAsync());
                    Logger.Add(LogLevel.Fatal, "HTTP Server", $"HTTP Context not found.");
                }
            }

            // On Cancellation
            _requestThreads.Clear();

            // Dispose Token
            this.cToken = null;
        }

        /// <summary>
        /// Handle HTTP Request.
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns>(void *async)</returns>
        public async Task RequestContextAsync(HttpListenerContext context)
        {
            // Peel out the requests and response objects
            HttpListenerRequest? request = context.Request;
            HttpListenerResponse? response = context.Response;

            // Local variable to manage served state
            bool served = false;

            // Create a Request Unique ID (For Logging Identification)
            string requestUID = Guid.NewGuid().ToString().Substring(0, 8);

            // Print out some info about the request
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"URL:<{request.Url}>", requestUID);
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"HttpMethod:<{request.HttpMethod}>", requestUID);
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"UserHostName: <{request.UserHostName}>", requestUID);
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"UserAgent: <{request.UserAgent}>", requestUID);
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"Operative System: <{Utils.GetOSFromUserAgent(request.UserAgent)}>", requestUID);            
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"AbsolutePath: <{request.Url.AbsolutePath}>", requestUID);
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"LocalPath: <{request.Url.LocalPath}>", requestUID);
            Logger.Add(LogLevel.OnlyDebug, "HTTP Server", $"AbsoluteUri: <{request.Url.AbsoluteUri}>", requestUID);

            // Serve the requested Path or Not found
            string requestedPath = request.Url.AbsolutePath;
            NameValueCollection requestedParams = GetRequestParams(request, requestUID);

            // Allow Cross-Origin Resource Sharing (CORS)
            if (allowCrossOrigin)
            {
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "POST, GET");
            }

            // Response: Custom Response Synchronous (API Call)
            if (_RoutesList.ContainsKey(requestedPath))
            {
                Func<NameValueCollection, string> callback = _RoutesList[requestedPath].callback;
                string mimeType = _RoutesList[requestedPath].mimeType;
                string content = callback(requestedParams);
                served = await ServeResponseCustomContent(response, requestedPath, content, mimeType, requestUID);
            }
            // Response: Custom Response Asynchronous (API Call)
            else if (_RoutesListAsync.ContainsKey(requestedPath))
            {
                Func<NameValueCollection, Task<string>> callback = _RoutesListAsync[requestedPath].callback;
                string mimeType = _RoutesListAsync[requestedPath].mimeType;
                string content = await callback(requestedParams);
                served = await ServeResponseCustomContent(response, requestedPath,content, mimeType, requestUID);
            }
            // Response: File on Embedded Resources (or) Response: File on App Folder
            else
            {
                // Get Local File or Default Local
                string? localFilePath = GetLocalFilePath(requestedPath, this.basePathLocal, requestUID);
                if (String.IsNullOrEmpty(localFilePath))
                    localFilePath = (!requestedPath.Contains('.')) ? GetLocalDefaultFilePath(requestedPath, this.basePathLocal, requestUID) : null;

                // Get Embeded File or Default Embedded (ON CALLER APP)
                string? embeddedFilePath = GetEmbeddedFilePath(requestedPath, this.basePathEmbedded, true, requestUID);
                if (String.IsNullOrEmpty(embeddedFilePath))
                    embeddedFilePath = (!requestedPath.Contains('.')) ? GetEmbeddedDefaultFilePath(requestedPath, this.basePathEmbedded, true, requestUID) : null;

                // Get Embeded File or Default Embedded (ON LIBRARY)
                string? embeddedFilePathLib = GetEmbeddedFilePath(requestedPath, this.basePathEmbedded, false, requestUID);
                if (String.IsNullOrEmpty(embeddedFilePathLib))
                    embeddedFilePathLib = (!requestedPath.Contains('.')) ? GetEmbeddedDefaultFilePath(requestedPath, this.basePathEmbedded, false, requestUID) : null;

                // Serve: Local File (If priority first) > Embedded Resource(App) > Embedded Resource(Library) > Local File
                if (ServeFirstLocal && !String.IsNullOrEmpty(localFilePath)) // Local File
                {
                    served = await ServeResponseLocalFile(response, localFilePath, requestUID);
                }
                else if (!String.IsNullOrEmpty(embeddedFilePath)) // Embeded File (ON CALLER APP)
                {
                    served = await ServeReponseEmbeddedFile(response, embeddedFilePath, true, requestUID);
                }
                else if (!String.IsNullOrEmpty(embeddedFilePathLib)) // Embeded File (ON LIBRARY)
                {
                    served = await ServeReponseEmbeddedFile(response, embeddedFilePathLib, false, requestUID);
                }
                else
                {
                    if (!String.IsNullOrEmpty(localFilePath))
                        served = await ServeResponseLocalFile(response, localFilePath, requestUID);
                }
            }
            // If Not Served (Not Found)
            if (!served)
            {
                served = await ServeReponseErrorNotFound(requestedPath, this.basePathEmbedded, true, this.basePathLocal, response, requestUID);
                if (!served) // (ON LIBRARY)
                    served = await ServeReponseErrorNotFound(requestedPath, this.basePathEmbedded, false, this.basePathLocal, response, requestUID);
            }
            // If Not Served (Server Error)
            if (!served)
            {
                served = await ServeReponseErrorServer(requestedPath, this.basePathEmbedded, true, this.basePathLocal, response, requestUID);
                if (!served) // (ON LIBRARY)
                    served = await ServeReponseErrorServer(requestedPath, this.basePathEmbedded, false, this.basePathLocal, response, requestUID);
            }
            // Close Response
            response.Close();
        }

        /// <summary>
        /// Stop the HTTP Server.
        /// </summary>
        /// <returns>(void *async)</returns>
        public void Stop()
        {
            if (!this.isRunning || _httpServerListener == null)
            {
                Logger.Add(LogLevel.Info, "HTTP Server", $"Server not running.");
                return;
            }
            try
            {
                if (cToken != null)
                    cToken.Cancel();
                _httpServerListener?.Stop();
                _httpServerListener?.Close();
                //cToken = null;
                Logger.Add(LogLevel.Info, "HTTP Server", $"Server Stopped and Closed.");
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "HTTP Server", $"Error stopping HTTP Server", $"Exception: {ex.Message}.");
            }
            NotifyChange("stopped");
        }

        #endregion

        #region Add Routes for Custom Responses (API Calls)

        /// <summary>
        /// Add a Route, defining an action to be triggered on request.<br/>
        /// <example>
        /// Example:<br/>
        /// **Defining Route**<br/>
        /// AddRoute("/custom/path", FunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public string FunctionToExecute(NameValueCollection requestParams)<br/>{<br/>return requestParams.ToString();<br/>}
        /// </example>
        /// </summary>
        /// <param name="path">Requested Path</param>
        /// <param name="callback">Function name to be called. (Note: Function should have *NameValueCollection* as input variable (requestParams). Return should be of type string.)</param>
        /// <param name="mimeType">Custom Mime Type (Default: text/html)</param>
        /// <returns>(void)</returns>
        public void AddRoute(string path, Func<NameValueCollection, string>? callback = null, string mimeType = "text/html")
        {
            LightHTTPServerRoute<string> response;
            response.callback = callback;
            response.mimeType = mimeType;
            if (!_RoutesList.ContainsKey(path))
                _RoutesList.Add(path, response);
            else
                Logger.Add(LogLevel.Warning, "HTTP Server", $"Route duplicated: <{path}>.");
        }

        /// <summary>
        /// Add a Route, defining an (async) action to be triggered on request.<br/>
        /// <example>
        /// Example:<br/>
        /// **Defining Route**<br/>
        /// AddRouteAsync("/custom/path", AsyncFunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public Task&lt;string&gt; AsyncFunctionToExecute(NameValueCollection requestParams)<br/>{<br/>return requestParams.ToString();<br/>}
        /// </example>
        /// </summary>
        /// <param name="path">Requested Path</param>
        /// <param name="callback">Function name to be called. (Note: Async Function should have *NameValueCollection* as input variable (requestParams). Return should be of type string.)</param>
        /// <param name="mimeType">Custom Mime Type (Default: text/html)</param>
        /// <returns>(void)</returns>
        public void AddRouteAsync(string path, Func<NameValueCollection, Task<string>>? callback = null, string mimeType = "text/html")
        {
            LightHTTPServerRoute<Task<string>> response;
            response.callback = callback;
            response.mimeType = mimeType;
            if (!_RoutesListAsync.ContainsKey(path))
                _RoutesListAsync.Add(path, response);
            else
                Logger.Add(LogLevel.Warning, "HTTP Server", $"Route Async duplicated: <{path}>.");
        }

        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on HTTP Server.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType)
        {
            this.isRunning = (_httpServerListener != null) ? _httpServerListener.IsListening : false;
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new LightHTTPServerEvent(eventType));
            }
        }

        #endregion

    }
}