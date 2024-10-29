using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;

namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class) Light ClickHouse Client <br/>
    /// Class to execute a ClickHouse Client.<br/><br/>
    /// **Important**<br/>
    /// Nugget Package Required: ClickHouse.Client<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.18 - July 18, 2024
    /// </remarks>
    public class ClickHouseClient
    {

        #region Properties

        private ClickHouseConnection? _connection;

        public string server { get; set; } = string.Empty;
        public int port { get; set; } = 0;
        public string database { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string lastError { get; set; } = string.Empty;
        public long lastExec { get; set; } = 0;
        public bool connected { get; set; } = false;
        public string baseURL { get => string.Format("{0}://{1}:{2}", DBTypes.ClickHouse.ToString().ToLower(), this.server, this.port.ToString()); }

        public event EventHandler<ClickHouseClientEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a Light ClickHouse Client
        /// </summary>
        /// <param name="server">ClickHouse Server</param>
        /// <param name="port">ClickHouse Port</param>
        /// <param name="database">ClickHouse Database</param>
        /// <param name="username">ClickHouse Username</param>
        /// <param name="password">ClickHouse Password</param>
        /// <param name="autostart">Start sever on creation</param>
        public ClickHouseClient(string server = "", int port = 8443, string database = "", string username = "", string password = "", bool autostart = true)
        {
            this.server = server;
            this.port = port;
            this.database = database;
            this.username = username;
            this.password = password;
            Logger.Add(LogLevel.OnlyDebug, "ClickHouse Client", $"ClickHouse Client created. Base URL: <{baseURL}>.", username);
            if (autostart)
                this.Connect();
        }

        /// <summary>
        /// Update settings of ClickHouse Client
        /// </summary>
        /// <param name="server">ClickHouse Server</param>
        /// <param name="port">ClickHouse Port</param>
        /// <param name="database">ClickHouse Database</param>
        /// <param name="username">ClickHouse Username</param>
        /// <param name="password">ClickHouse Password</param>
        /// <param name="restart">Restart Server on Update</param>
        /// <returns>(void)</returns>
        public async Task Update(string server = "", int port = 8443, string database = "", string username = "", string password = "", bool restart = true)
        {
            if (restart)
                await this.Disconnect();
            this.server = server;
            this.port = port;
            this.database = database;
            this.username = username;
            this.password = password;
            if (restart)
                await this.Connect();
            Logger.Add(LogLevel.OnlyDebug, "ClickHouse Client", $"Updated Settings. Base URL: <{baseURL}>.", username);
        }

        #endregion

        #region Connect / Disconnect

        /// <summary>
        /// Connect to a ClickHouse Server (Database)
        /// </summary>
        /// <returns>(bool *async) Succeded / Failed</returns>
        public async Task<bool> Connect()
        {
            // Already Connected
            if (this.connected && _connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    lastError = $"";
                    Logger.Add(LogLevel.Info, "ClickHouse Client", $"Connection already stablished. Base URL: <{baseURL}>.", username);
                    return true;
                }
                else
                {
                    lastError = $"Error connecting ClickHouse Database. Connection Status: {_connection.State.ToString()}.";
                    Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                    return false;
                }
            }

            // Check Connection Variables
            if (string.IsNullOrEmpty(this.server))
            {
                lastError = $"Error: No Server name declared.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return false;
            }
            if (this.port == 0)
            {
                lastError = $"Error: No Port declared.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return false;
            }
            if (string.IsNullOrEmpty(this.database))
            {
                lastError = $"Error: No database name declared.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return false;
            }
            if (string.IsNullOrEmpty(this.username))
            {
                lastError = $"Error: No username declared.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return false;
            }
            if (string.IsNullOrEmpty(this.password))
            {
                lastError = $"Error: No password found.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return false;
            }

            // Execute Connection
            try
            {
                string connectionString = string.Format("Host={0}; Port={1}; Database={2}; Username={3}; Password={4}", this.server, this.port, this.database, this.username, this.password);
                _connection = new ClickHouseConnection(connectionString);
                await _connection.OpenAsync();
                lastError = "";
                Logger.Add(LogLevel.Info, "ClickHouse Client", $"Connection stablished <Server={this.server} | Port={this.port} | Database={this.database}>.", username);
                NotifyChange("connected");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Error connecting ClickHouse Database. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                _connection = null;
                NotifyChange("disconnected");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from a ClickHouse Server (Database)
        /// </summary>
        /// <returns>(bool *async) Succeded / Failed</returns>
        public async Task<bool> Disconnect()
        {
            // Already disconnected
            if (!this.connected || _connection == null)
            {
                Logger.Add(LogLevel.Info, "ClickHouse Client", $"ClickHouse Client already disconnected.", username);
                return true;
            }

            // Execute Disconnection
            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
                Logger.Add(LogLevel.Info, "ClickHouse Client", $"Connection Disconnected <Server={this.server} | Port={this.port} | Database={this.database}>.", username);
                NotifyChange("disconnected");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Error closing connection ClickHouse Database. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                NotifyChange("undefined");
                return false;
            }
        }

        #endregion

        #region Queries Wrappers

        /// <summary>
        /// NonQuery = to insert, update, and delete, (create, set) data.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <returns>(int *async) Rows affected</returns>
        public async Task<int?> NonQueryAsync(string query)
        {
            // Check connection
            if (!this.connected || _connection == null)
            {
                this.lastError = $"Error during querying database. No connection found.";
                Logger.Add(LogLevel.Fatal, "ClickHouse Client", $"{lastError}", username);
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ClickHouseCommand cmd = new ClickHouseCommand(_connection);
                cmd.CommandText = query;
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "ClickHouse Client", $"(Non)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms. Rows affected: {affectedRows}.", username);
                return affectedRows;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Non)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return null;
            }
        }

        /// <summary>
        /// Reader = to query the database: select.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <returns>(DbDataReader *async)</returns>
        public async Task<DbDataReader?> ReaderQueryAsync(string query)
        {
            // Check connection
            if (!this.connected || _connection == null)
            {
                this.lastError = $"Error during querying database. No connection found.";
                Logger.Add(LogLevel.Fatal, "ClickHouse Client", $"{lastError}", username);;
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ClickHouseCommand cmd = new ClickHouseCommand(_connection);
                cmd.CommandText = query;
                DbDataReader? reader = await cmd.ExecuteReaderAsync();
                //reader.Close();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "ClickHouse Client", $"(Reader)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms. Rows affected: {reader.RecordsAffected}.", username);
                return reader;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Reader)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return null;
            }
        }

        /// <summary>
        /// Scalar = to return a single value.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <returns>(Object *async)</returns>
        public async Task<object?> ScalarQueryAsync(string query)
        {
            // Check connection
            if (!this.connected || _connection == null)
            {
                this.lastError = $"Error during querying database. No connection found.";
                Logger.Add(LogLevel.Fatal, "ClickHouse Client", $"{lastError}", username);
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ClickHouseCommand cmd = new ClickHouseCommand(_connection);
                cmd.CommandText = query;
                object? result = await cmd.ExecuteScalarAsync();
                //reader.Close();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "ClickHouse Client", $"(Scalar)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms.", username);
                return result;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Scalar)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "ClickHouse Client", $"{lastError}", username);
                return null;
            }
        }


        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on Lite ClickHouse Client.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType)
        {
            this.connected = (_connection != null) ? (_connection.State == ConnectionState.Open) : false;
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new ClickHouseClientEvent(eventType));
            }
        }

        #endregion

    }
}
