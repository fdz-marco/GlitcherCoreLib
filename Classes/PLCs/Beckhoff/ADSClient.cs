using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using TwinCAT.Ads.TcpRouter;
using glitcher.core;

namespace glitcher.core.PLC.Beckhoff
{
    /// <summary>
    /// (Class) TwinCAT ADS Client <br/>
    /// Class to connect via ADS to Beckhoff TwinCAT PLCs.<br/><br/>
    /// **Important**<br/>
    /// Nugget Package Required: Beckhoff.TwinCAT.Ads<br/>
    /// Nugget Package Required: Beckhoff.TwinCAT.Ads.TcpRouter<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.04.26 - April 04, 2024
    /// </remarks>
    public class ADSClient
    {

        #region Properties

        /*  *
         *  Beckhoff Infosys Info
         *  https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_adssamples_net/185250827.html&id=
         *  https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_adssamples_net/185255435.html&id=
         *  https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_adsnetref/7312831627.html&id=
        * */

        private AdsClient? _tcADSClient = null;
        private AmsTcpIpRouter? _tcADSRouter = null;
        private List<ISymbol>? _symbols = null;
        private string _clientAdress = String.Empty;
        //private ISymbolLoader _TcADSSymbolLoader = null;

        public string host { get; set; } = "127.0.0.1";
        public int port { get; set; } = 851;
        public string AMSNetID { get; set; } = "127.0.0.1.1.1";
        public bool connected { get; set; } = false;
        public string baseURL { get => string.Format("{0}:{1}", this.host, this.port.ToString()); }        
        public List<uint> notificationHandlers { get; set; } = new List<uint>();
        public List<TagSubscribed> tagsSubscribed { get; set; } = new List<TagSubscribed>();
        public string clientId = "";

        public event EventHandler<ADSClientEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a ADS Client
        /// </summary>
        /// <param name="host">PLC IP Address</param>
        /// <param name="port">PLC Run-time Port</param>
        /// <param name="AMSNetID">PLC AMS NetID</param>
        /// <param name="autostart">Start client on Creation</param>
        public ADSClient(string host = "127.0.0.1", int port = 851, string AMSNetID = "127.0.0.1.1.1", bool autostart = true)
        {
            this.host = host;
            this.port = port;
            this.AMSNetID = AMSNetID;
            this.clientId = Guid.NewGuid().ToString().Substring(0, 6);
            if (autostart)
                this.Connect();
            Logger.Add(LogLevel.OnlyDebug, "ADS Client", $"ADS Client created. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
        }
        /// <summary>
        /// Update settings of ADS Client
        /// </summary>
        /// <param name="host">PLC IP Address</param>
        /// <param name="port">PLC Run-time Port</param>
        /// <param name="AMSNetID">PLC AMS NetID</param>>
        /// <param name="restart">Restart client on Update</param>
        /// <returns>(void)</returns>
        public async Task Update(string host = "127.0.0.1", int port = 851, string AMSNetID = "127.0.0.1.1.1", bool restart = true)
        {
            if (restart)
                await this.Disconnect();
            this.host = host;
            this.port = port;
            this.AMSNetID = AMSNetID;
            if (restart)
                await this.Connect();
            Logger.Add(LogLevel.Info, "ADS Client", $"Updated Settings. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
        }

        #endregion

        #region Connect / Disconnnect

        /// <summary>
        /// Connect to a PLC via ADS
        /// </summary>
        /// <returns>(bool *async) Succeded / Failed</returns>
        public async Task<bool> Connect(bool wRouter = true)
        {
            // Already Connected
            if (this.connected && _tcADSClient != null)
            {
                Logger.Add(LogLevel.Info, "ADS Client", $"Connection already stablished. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                return true;
            }

            // Execute Connection
            try
            {
                
                _tcADSClient = new AdsClient();

                if (wRouter)
                    await CreateRouter();

                AmsAddress _amsAddress = new AmsAddress(AMSNetID, port);
                _tcADSClient.Connect(AMSNetID, port); 
                //await _tcADSClient.ConnectAndWaitAsync(_amsAddress, CancellationToken.None);
                if (_tcADSClient.IsConnected)
                {
                    if (_tcADSClient.ClientAddress != null)
                    {
                        _clientAdress = _tcADSClient.ClientAddress.ToString();
                        if (String.IsNullOrEmpty(_clientAdress))
                            throw new Exception("Wrong connection parameters");
                    }
                    Logger.Add(LogLevel.Success, "ADS Client", $"Client Connected: <{_clientAdress}>. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                    NotifyChange("connected");
                    return true;
                }
                throw new Exception("Wrong connection parameters");
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "ADS Client", $"Error connecting ADS. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>. Exception: {ex.Message}", clientId);
                _tcADSClient = null;
                NotifyChange("disconnected");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from a PLC via ADS
        /// </summary>
        /// <returns>(bool *async) Succeded / Failed</returns>
        public async Task<bool> Disconnect()
        {
            // Already disconnected
            if (!this.connected || _tcADSClient == null)
            {
                Logger.Add(LogLevel.Info, "ADS Client", $"ADS Client already disconnected.", clientId);
                return true;
            }

            // Execute Disconnection
            try
            {
                foreach (var item in notificationHandlers)
                {
                    await _tcADSClient.DeleteDeviceNotificationAsync(item, CancellationToken.None);
                }
                notificationHandlers.Clear();
                tagsSubscribed.Clear();
                _tcADSClient.Disconnect();
                _tcADSClient.Close();
                _tcADSClient.Dispose();
                _tcADSClient = null;
                Logger.Add(LogLevel.Info, "ADS Client", $"ADS Client Disconnected. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                NotifyChange("disconnected");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Info, "ADS Client", $"Error closing ADS Client connection. Exception: {ex.Message}.", clientId);
                NotifyChange("undefined");
                return false;
            }
        }

        #endregion

        #region Router

        public async Task<bool> CreateRouter(CancellationToken? ct = null)
        {
            try
            {
                // Remote and Local Addreses
                string _localIP = "10.20.30.10";
                string _remoteIP = this.host.Trim();
                AmsNetId _localAmsNetID = new AmsNetId($"{_localIP}.1.1");
                AmsNetId _remoteAmsNetID = new AmsNetId(this.AMSNetID.Trim());

                // Create router
                _tcADSRouter = new AmsTcpIpRouter(_localAmsNetID);

                // Cancellation Token
                if (ct == null)
                    ct = CancellationToken.None;

                // Add Route
                Route _route = new Route("ADSConnection", _remoteAmsNetID, _remoteIP);
                _tcADSRouter.AddRoute(_route);
                await _tcADSRouter.StartAsync((CancellationToken)ct);
                Logger.Add(LogLevel.Success, "ADS Client", $"ADS Router created and Route Added. Local IP: <{_localIP}>. Local AMSNetID: <{_localAmsNetID}>. Remote IP: <{_remoteIP}>. Remote AMSNetID: <{_remoteAmsNetID}>.", clientId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Fatal, "ADS Client", $"ADS Router creation Failed. Exception: {ex.Message}.", clientId);
                return false;
            }
        }

        #endregion

        #region Tags Read

        /// <summary>
        /// Read a Variable
        /// </summary>
        /// <param name="variableName">Variable Name</param>
        /// <param name="datatype">Data Type</param>
        /// <param name="size">Size</param>
        /// <returns>(dynamic) Value of Variable</returns>
        public dynamic? ReadVariable(string variableName, string datatype, int size = 0)
        {
            // Check connection
            if (!this.connected || _tcADSClient == null)
            {
                Logger.Add(LogLevel.Fatal, "ADS Client", $"ADS Client not connected. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                return null;
            }

            // Execute Task
            uint varHandle = 0;
            try
            {

                varHandle = _tcADSClient.CreateVariableHandle(variableName);
                Type type = TagSubscribed.GetTypeFromString(datatype, size);
                dynamic? value = "";

                if (datatype.StartsWith("STRING") || datatype.StartsWith("WSTRING"))
                {
                    value = _tcADSClient.ReadAny(varHandle, type, new int[] { size });
                }
                else
                {
                    value = _tcADSClient.ReadAny(varHandle, type);
                }
                Logger.Add(LogLevel.OnlyDebug, "ADS Client", $"Variable Readed. <{variableName} ({datatype}[{size}]) = {value}>.", clientId);
                return value;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "ADS Client", $"Error Reading Variable. <{variableName} ({datatype}[{size}])>. Exception: {ex.Message}.", clientId);
            }
            finally
            {
                if (varHandle != 0)
                {
                    _tcADSClient.DeleteVariableHandle(varHandle);
                }
            }
            return "";
        }

        /// <summary>
        /// Read a Variable (Async)
        /// </summary>
        /// <param name="variableName">Variable Name</param>
        /// <param name="datatype">Data Type</param>
        /// <param name="size">Size</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>(dynamic) Value of Variable</returns>
        public async Task<dynamic?> ReadVariableAsync(string variableName, string datatype, int size = 0, CancellationToken? ct = null)
        {
            // Check connection
            if (!this.connected || _tcADSClient == null)
            {
                Logger.Add(LogLevel.Fatal, "ADS Client", $"ADS Client not connected. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                return null;
            }

            // Cancellation Token
            if (ct == null)
                ct = CancellationToken.None;

            // Execute Task
            uint varHandle = 0;
            try
            {
                ResultHandle _varHandle = await _tcADSClient.CreateVariableHandleAsync(variableName, (CancellationToken)ct);
                varHandle = _varHandle.Handle;
                Type type = TagSubscribed.GetTypeFromString(datatype, size);
                dynamic? value = "";

                if (datatype.StartsWith("STRING") || datatype.StartsWith("WSTRING"))
                {
                    ResultAnyValue result = await _tcADSClient.ReadAnyAsync(varHandle, type, new int[] { size }, (CancellationToken)ct);
                    value = result.Value;
                }
                else
                {
                    ResultAnyValue result = await _tcADSClient.ReadAnyAsync(varHandle, type, (CancellationToken)ct);
                    value = result.Value;
                }
                Logger.Add(LogLevel.OnlyDebug, "ADS Client", $"Variable Readed (Async). <{variableName} ({datatype}[{size}]) = {value}>.");
                return value;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "ADS Client", $"Error Reading Variable (Async). <{variableName} ({datatype}[{size}])>. Exception: {ex.Message}.");
            }
            finally
            {
                if (varHandle != 0)
                {
                    await _tcADSClient.DeleteVariableHandleAsync(varHandle, (CancellationToken)ct);
                }
            }
            return "";
        }

        #endregion

        #region Tags Subscription

        /// <summary>
        /// Subscribe to a Variable, defining an action to be triggered on update.<br/>
        /// <example>
        /// Example:<br/>
        /// **Subscribe to a Variable**<br/>
        /// SubscribeVariable("PLC.Main.Var1", "STRING", 255, 200, 0, FunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public void FunctionToExecute(string tagPath, string valueString)<br/>{<br/>return;<br/>}
        /// </example>
        /// </summary>
        /// <param name="variableName">Variable Name</param>
        /// <param name="datatype">Data Type</param>
        /// <param name="size">Size</param>
        /// <param name="cycleTime">Cycle Time</param>
        /// <param name="maxDelay">Max Delay</param>
        /// <param name="callback">Function name to be called. (Note: Function should have *string tagPath, string valueString* as input variables. Return should be of type void.</param>
        /// <returns>(bool) Succeded / Failed</returns>
        public bool SubscribeVariable(string variableName, string datatype, int size = 0, int cycleTime = 200, int maxDelay = 0, Action<string, string>? callback = null)
        {
            // Check connection
            if (!this.connected || _tcADSClient == null)
            {
                Logger.Add(LogLevel.Fatal, "ADS Client", $"ADS Client not connected. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                return false;
            }

            // Check if variable already subscribed
            TagSubscribed? tagItem = tagsSubscribed.FirstOrDefault(tag => tag?.tagPath == variableName, null);
            if (tagItem != null)
            {
                Logger.Add(LogLevel.Info, "ADS Client", $"Variable already subcribed. <Var: {tagItem.tagPath} ({tagItem.datatype}[{tagItem.size}]) | CycleTime: {tagItem.cycleTime}ms | MaxDelay: {tagItem.maxDelay}ms>.", clientId);
                return true;
            }

            // Execute Task
            uint notificationHandle = 0;
            try
            {
                NotificationSettings settings = new NotificationSettings(AdsTransMode.OnChange, cycleTime, maxDelay);
                Type type = TagSubscribed.GetTypeFromString(datatype, size);

                if (datatype.StartsWith("STRING") || datatype.StartsWith("WSTRING"))
                {
                    notificationHandle = _tcADSClient.AddDeviceNotificationEx(variableName, settings, new { callback = callback }, type, new int[] { size });
                }
                else
                {
                    notificationHandle = _tcADSClient.AddDeviceNotificationEx(variableName, settings, new { callback = callback }, type);
                }

                notificationHandlers.Add(notificationHandle);
                tagsSubscribed.Add(new TagSubscribed(notificationHandle, variableName, datatype, size, cycleTime, maxDelay, null));
                _tcADSClient.AdsNotificationEx += SubcriptionVariableHandler;
                Logger.Add(LogLevel.Info, "ADS Client", $"Variable Subcribed. <{variableName} ({datatype}[{size}]) | CycleTime: {cycleTime}ms | MaxDelay: {maxDelay}ms>.", clientId);
                return true;
            }
            catch (Exception ex)
            {
                if (notificationHandle != 0)
                {
                    _tcADSClient.DeleteDeviceNotification(notificationHandle);
                }
                _tcADSClient.AdsNotificationEx -= SubcriptionVariableHandler;
                Logger.Add(LogLevel.Error, "ADS Client", $"Error Subscribing Variable. <{variableName} ({datatype}[{size}]) | CycleTime: {cycleTime}ms | MaxDelay: {maxDelay}ms>. Exception: {ex.Message}.", clientId);
                return false;
            }
        }

        /// <summary>
        /// Subscription to Variable Handler (On Update)
        /// </summary>
        /// <param name="sender">Sender Object</param>
        /// <param name="e">Event Arguments</param>
        /// <returns>(void *async)</returns>
        public async void SubcriptionVariableHandler(object? sender, AdsNotificationExEventArgs e)
        {
            // Get event data
            uint handler = e.Handle;
            ReadOnlyMemory<byte> data = e.Data;
            DateTime timeStamp = e.TimeStamp.DateTime;
            dynamic? value = e.Value;

            // Get TagName and Update Subscribed Tags List
            TagSubscribed? tagItem = tagsSubscribed.FirstOrDefault(tag => tag?.handler == handler, null);
            if (tagItem == null) return;

            int index = tagsSubscribed.IndexOf(tagItem);
            string tagPath = tagsSubscribed[index].tagPath;
            string? valueString = tagsSubscribed[index].valueString;
            bool tagUpdated = await tagsSubscribed[index].UpdateValue(value, timeStamp);

            // Call Callback
            if ((e.UserData != null) && (tagUpdated))
            {
                try
                {
                    // Get parameters from anonymous class
                    Object input = e.UserData;
                    Action<string, string?> callback = (Action<string, string?>)input.GetType().GetProperty("callback").GetValue(input, null);
                    // Execute callback and pass value of variabel changed as argument
                    callback(tagPath, valueString);
                    Logger.Add(LogLevel.OnlyDebug, "ADS Client", $"Subscription Variable Handler. Updated Succeed. <{tagPath}={valueString}>.", clientId);
                }
                catch (Exception ex)
                {
                    Logger.Add(LogLevel.Error, "ADS Client", $"Subscription Variable Handler. Updated Failed. <{tagPath}={valueString}>. Exception: {ex.Message}.", clientId);
                }
            }
        }

        /// <summary>
        /// Read Variable and Subscribe to a Variable, defining an action to be triggered on update.<br/>
        /// <example>
        /// Example:<br/>
        /// **Subscribe to a Variable**<br/>
        /// ReadSubscribe("PLC.Main.Var1", "STRING", 255, 200, 0, FunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public void FunctionToExecute(string tagPath, string valueString)<br/>{<br/>return;<br/>}
        /// </example>
        /// </summary>
        /// <param name="variableName">Variable Name</param>
        /// <param name="datatype">Data Type</param>
        /// <param name="size">Size</param>
        /// <param name="cycleTime">Cycle Time</param>
        /// <param name="maxDelay">Max Delay</param>
        /// <param name="callback">Function name to be called. (Note: Function should have *string tagPath, string valueString* as input variables. Return should be of type void.</param>
        /// <returns>(dynamic) Value of Variable</returns>
        public dynamic? ReadSubscribe(string variableName, string datatype, int size = 0, int cycleTime = 200, int maxDelay = 0, Action<string, string>? callback = null)
        {
            dynamic? value = ReadVariable(variableName, datatype, size);
            SubscribeVariable(variableName, datatype, size, cycleTime, maxDelay, callback);
            return value;
        }

        #endregion

        #region Get Symbols

        /// <summary>
        /// Get Symbols (Async)
        /// </summary>
        /// <param name="forceUpdate">Force Update of Symbols</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>(List<ISymbol>) List of Symbols</returns>
        public async Task<List<ISymbol>?> GetSymbols(bool forceUpdate = false, CancellationToken? ct = null)
        {
            // Check connection
            if (!this.connected || _tcADSClient == null)
            {
                Logger.Add(LogLevel.Fatal, "ADS Client", $"ADS Client not connected. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                return null;
            }

            // Cancellation Token
            if (ct == null)
                ct = CancellationToken.None;

            // Execute Task: Get Symbols
            try
            {
                if ((this._symbols == null) || (forceUpdate == true))
                {
                    IDynamicSymbolLoader _symbolsLoader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(this._tcADSClient, SymbolLoaderSettings.DefaultDynamic);
                    ResultDynamicSymbols _symbolsResult = await _symbolsLoader.GetDynamicSymbolsAsync((CancellationToken)ct);
                    if (_symbolsResult.Failed == false && _symbolsResult.Symbols != null)
                    {
                        IDynamicSymbolsCollection _symbolsDynamic = _symbolsResult.Symbols;
                        this._symbols = _symbolsDynamic.ToList();
                        Logger.Add(LogLevel.Success, "ADS Client", $"Get Symbols successful. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                    }
                    else
                    {
                        Logger.Add(LogLevel.Error, "ADS Client", $"Get Symbols failed. Empty symbol list. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                    }
                }
                else
                {
                    Logger.Add(LogLevel.OnlyDebug, "ADS Client", $"Get Symbols already executed. Retrieving saved values. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>.", clientId);
                }
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "ADS Client", $"Get Symbols failed. Base URL: <{baseURL}>. AMSNet ID: <{this.AMSNetID}>. Exception: {ex.Message}", clientId);
            }
            return this._symbols;
        }

        #endregion

        #region Others

        /// <summary>
        /// Get Client Address
        /// </summary>
        /// <returns>(string)Client Address</returns>
        public string GetClientAddress()
        {
            return _clientAdress;
        }

        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on ADS Client.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType)
        {
            this.connected = (_tcADSClient != null) ? _tcADSClient.IsConnected : false;
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new ADSClientEvent(eventType));
            }
        }

        #endregion

    }
}
