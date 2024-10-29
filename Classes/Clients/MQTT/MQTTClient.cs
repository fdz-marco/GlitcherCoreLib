using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Text;

namespace glitcher.core.Clients
{
    /// <summary>
    /// (Class) MQTT Client <br/>
    /// Class to connect to a MQTT Broker.<br/>
    /// **Important**<br/>
    /// NuggetPackage required: MQTTnet
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.10.28 - October 28, 2024
    /// </remarks>
    public class MQTTClient
    {
        #region Properties

        private MqttFactory? _mqttFactory = null;
        private IMqttClient? _mqttClient = null;

        public string host { get; set; } = "127.0.0.1";
        public int port { get; set; } = 1883;
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public enum Protocol { mqtt, ws };
        public Protocol protocol { get; set; } = Protocol.mqtt;
        public bool connected { get; set; } = false;
        public string baseURL { get => string.Format("{0}://{1}:{2}", this.protocol.ToString(), this.host, this.port.ToString()); }
        public string clientId = "";
        public List<TopicSubscribed> topicsSubscribed { get; set; } = new List<TopicSubscribed>();
        public EventHandler<MQTTClientEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a MQTT Client
        /// </summary>
        /// <param name="host">MQTT Broker Host</param>
        /// <param name="port">MQTT Broker Port (Default: 1883)</param>
        /// <param name="username">Username (Default: empty)</param>
        /// <param name="password">Password (Default: empty).</param>
        /// <param name="protocol">Protocol to connect (Default: MQTT)<param>
        /// <param name="autostart">Start client on creation</param>
        /// <returns>(void)</returns>
        public MQTTClient(string host, int port = 1883, string username = "", string password = "", Protocol protocol = Protocol.mqtt, bool autostart = true)
        {
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
            this.protocol = protocol;
            this.clientId = Guid.NewGuid().ToString();
            Logger.Add(LogLevel.OnlyDebug, "MQTT Client", $"MQTT Client created. Base URL: <{baseURL}>. ClientId: <{clientId}>.", clientId);
            if (autostart)
                this.Start();
        }

        /// <summary>
        /// Update settings of MQTT Client
        /// </summary>
        /// <param name="host">MQTT Broker Host</param>
        /// <param name="port">MQTT Broker Port (Default: 1883)</param>
        /// <param name="username">Username (Default: empty)</param>
        /// <param name="password">Password (Default: empty).</param>
        /// <param name="protocol">Protocol to connect (Default: MQTT)<param>
        /// <param name="restart">Restart Client on Update</param>
        /// <returns>(void)</returns>
        public async Task Update(string host, int port = 1883, string username = "", string password = "", Protocol protocol = Protocol.mqtt, bool restart = true)
        {
            if (restart)
                await this.Stop();
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
            this.protocol = protocol;
            this.clientId = Guid.NewGuid().ToString();
            if (restart)
                await this.Start();
            Logger.Add(LogLevel.Info, "MQTT Client", $"Updated Settings. Base URL: <{baseURL}>.");
        }

        #endregion

        #region Start / Stop

        /// <summary>
        /// Start the MQTT Client.
        /// </summary>
        /// <returns>(void *async)</returns>
        public async Task<bool> Start()
        {
            // Already Connected
            if (this.connected && _mqttFactory != null && _mqttClient != null)
            {
                Logger.Add(LogLevel.Warning, "MQTT Client", $"Connection already stablished. Connected on: {this.baseURL}.");
                return true;
            }

            // Execute Connection
            try
            {
                _mqttFactory = new MqttFactory();
                _mqttClient = _mqttFactory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(host, port)
                    .WithCredentials(username, password)
                    .WithClientId(clientId)
                    .WithCleanSession()
                .Build();

                var connection = await _mqttClient.ConnectAsync(options);
                if (connection.ResultCode == MqttClientConnectResultCode.Success)
                {
                    Logger.Add(LogLevel.Success, "MQTT Client", $"Client Connected. Broker={this.baseURL}");
                    this.connected = true;
                    NotifyChange("connected");
                    return true;
                }
                throw new Exception("Not success message returned on connection.");
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"Error connecting MQTT. Broker={this.baseURL}. Exception: {ex.Message}.");
                _mqttFactory = null;
                _mqttClient = null;
                this.connected = false;
                NotifyChange("disconnected");
                return false;
            }
        }

        /// <summary>
        /// Stop the MQTT Client.
        /// </summary>
        /// <returns>(void *async)</returns>
        public async Task<bool> Stop()
        {
            // Already disconnected
            if (!this.connected || _mqttFactory == null || _mqttClient == null)
            {
                Logger.Add(LogLevel.Warning, "MQTT Client", $"MQTT Client already disconnected.");
                return true;
            }

            // Execute Disconnection
            try
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
                _mqttClient = null;
                _mqttFactory = null;
                clientId = String.Empty;
                topicsSubscribed.Clear();
                Logger.Add(LogLevel.Info, "MQTT Client", $"Client Disconnected. Broker={this.baseURL}.");
                this.connected = false;
                NotifyChange("disconnected");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"Error closing connection. Broker={this.baseURL}. Exception: {ex.Message}.");
                NotifyChange("error");
                return false;
            }
        }

        #endregion

        #region Topic Subscription

        /// <summary>
        /// Subscribe to a topic, defining an action to be triggered on update.<br/>
        /// <example>
        /// Example:<br/>
        /// **Defining Route**<br/>
        /// SubscribeTopic("/topic/path/example", FunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public void FunctionToExecute(string path, string value)<br/>{<br/>return;<br/>}
        /// </example>
        /// </summary>
        /// <param name="path">Topic Path</param>
        /// <param name="callback">Function name to be called. (Note: Function should have *string topic, string value* as input variables. Return should be of type void.</param>
        /// <returns>Task(void)</returns>
        public async Task SubscribeTopic(string path, Action<string, string>? callback = null)
        {
            // Check connection
            if (!this.connected || _mqttFactory == null || _mqttClient == null)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"MQTT Client not connected.");
                return;
            }

            // Check if topic already subscribed
            TopicSubscribed? topicItem = topicsSubscribed.FirstOrDefault(topic => topic?.path == path, null);

            if (topicItem != null)
            {
                Logger.Add(LogLevel.Info, "MQTT Client", $"Topic already subcribed >> {path}.", clientId);
                return;
            }

            try
            {
                await Task.Run(async () =>
                {

                    var response = await _mqttClient.SubscribeAsync(path);
                    topicsSubscribed.Add(new TopicSubscribed(path, ""));

                    _mqttClient.ApplicationMessageReceivedAsync += (ev) =>
                    {
                        string topic = ev.ApplicationMessage.Topic;
                        string payload = Encoding.UTF8.GetString(ev.ApplicationMessage.PayloadSegment);

                        TopicSubscribed? topicItem = topicsSubscribed.FirstOrDefault(topic => topic?.path == path, null);

                        if (topicItem != null)
                        {
                            if (topicItem.payload == payload) return Task.CompletedTask;
                            topicItem.Update(payload);
                            int index = topicsSubscribed.IndexOf(topicItem);
                            topicsSubscribed[index] = topicItem;
                        }

                        if (callback != null) callback(topic, payload);
                        Logger.Add(LogLevel.OnlyDebug, "MQTT Client", $"Updated: {topic} => {payload}", clientId);
                        return Task.CompletedTask;
                    };

                    Logger.Add(LogLevel.Info, "MQTT Client", $"Topic Subcribed >> {path}", clientId);
                });
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"Error Subscribing Topic >> {path}. Exception: {ex.Message}", clientId);
            }
        }

        /// <summary>
        /// Publish to a topic, and trigger an action on update.<br/>
        /// <example>
        /// Example:<br/>
        /// **Defining Route**<br/>
        /// PublishTopic("/topic/path/example", "value", FunctionToExecute);<br/><br/>
        /// **Defining Function**<br/>
        /// public void FunctionToExecute(string path, string value)<br/>{<br/>return;<br/>}
        /// </example>
        /// </summary>
        /// <param name="path">Topic Path</param>
        /// <param name="payload">Topic Payload</param>
        /// <param name="callback">Function name to be called. (Note: Function should have *string topic, string value* as input variables. Return should be of type void.</param>
        /// <returns>Task(void)</returns>
        public async Task PublishTopic(string path, string payload, Action<string, string>? callback = null)
        {
            // Check connection
            if (!this.connected || _mqttFactory == null || _mqttClient == null)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"MQTT Client not connected.");
                return;
            }

            // Check if topic already subscribed
            TopicSubscribed? topicItem = topicsSubscribed.FirstOrDefault(topic => topic?.path == path, null);

            if (topicItem == null)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"Topic not subcribed >> {path}.", clientId);
                return;
            }

            try
            {
                await Task.Run(async () =>
                {
                    if (topicItem != null)
                    {
                        if (topicItem.payload == payload) return;

                        var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(path)
                        .WithPayload(payload)
                        .Build();

                        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                        topicItem.Update(payload);
                        int index = topicsSubscribed.IndexOf(topicItem);
                        topicsSubscribed[index] = topicItem;

                        if (callback != null) callback(path, payload);

                        Logger.Add(LogLevel.OnlyDebug, "MQTT Client", $"Published: {path} => {payload}", clientId);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "MQTT Client", $"Error Publishing Topic >> {path}. Exception: {ex.Message}", clientId);
            }
        }

        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on MQTT Client.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType)
        {
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new MQTTClientEvent(eventType));
            }
        }

        #endregion

    }
}

