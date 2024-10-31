using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class) Light MySQL Client <br/>
    /// Class to execute a MySQL Client.<br/><br/>
    /// **Important**<br/>
    /// Nugget Package Required: MySql.Data<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.18 - July 18, 2024
    /// </remarks>
    public class MySQLClient
    {

        #region Properties

        private MySqlConnection? _connection;

        public string server { get; set; } = string.Empty;
        public int port { get; set; } = 0;
        public string database { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string lastError { get; set; } = string.Empty;
        public long lastExec { get; set; } = 0;
        public bool connected { get; set; } = false;
        public string baseURL { get => string.Format("{0}://{1}:{2}", DBTypes.MySQL.ToString().ToLower(), this.server, this.port.ToString()); }

        public event EventHandler<MySQLClientEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a Light MySQL Client
        /// </summary>
        /// <param name="server">MySQL Server</param>
        /// <param name="port">MySQL Port</param>
        /// <param name="database">MySQL Database</param>
        /// <param name="username">MySQL Username</param>
        /// <param name="password">MySQL Password</param>
        /// <param name="autostart">Start sever on creation</param>
        public MySQLClient(string server = "", int port = 3306, string database = "", string username = "", string password = "", bool autostart = true)
        {
            this.server = server;
            this.port = port;
            this.database = database;
            this.username = username;
            this.password = password;
            Logger.Add(LogLevel.OnlyDebug, "MySQL Client", $"MySQL Client created. Base URL: <{baseURL}>.", username);
            if (autostart)
                this.Connect();
        }

        /// <summary>
        /// Update settings of MySQL Client
        /// </summary>
        /// <param name="server">MySQL Server</param>
        /// <param name="port">MySQL Port</param>
        /// <param name="database">MySQL Database</param>
        /// <param name="username">MySQL Username</param>
        /// <param name="password">MySQL Password</param>
        /// <param name="restart">Restart Server on Update</param>
        /// <returns>(void)</returns>
        public async Task Update(string server = "", int port = 3306, string database = "", string username = "", string password = "", bool restart = true)
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
            Logger.Add(LogLevel.OnlyDebug, "MySQL Client", $"Updated Settings. Base URL: <{baseURL}>.", username);
        }

        #endregion

        #region Connect / Disconnect

        /// <summary>
        /// Connect to a MySQL Server (Database)
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
                    Logger.Add(LogLevel.Info, "MySQL Client", $"Connection already stablished. Base URL: <{baseURL}>.", username);
                    return true;
                }
                else
                {
                    lastError = $"Error connecting MySQL Database. Connection Status: {_connection.State.ToString()}.";
                    Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                    return false;
                }
            }

            // Check Connection Variables
            if (string.IsNullOrEmpty(this.server))
            {
                lastError = $"Error: No Server name declared.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                return false;
            }
            if (this.port == 0)
            {
                lastError = $"Error: No Port declared.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                return false;
            }
            if (this.password == null)
            {
                lastError = $"Error: No database name declared.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                return false;
            }
            if (string.IsNullOrEmpty(this.username))
            {
                lastError = $"Error: No username declared.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                return false;
            }
            if (this.password == null)
            {
                lastError = $"Error: No password found.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                return false;
            }

            // Execute Connection
            try
            {
                string connectionString = string.Format("Server={0};Port={1};UID={2};Password={3};Database={4};", this.server, this.port, this.username, this.password, this.database);
                _connection = new MySqlConnection(connectionString);
                await _connection.OpenAsync();
                lastError = "";
                Logger.Add(LogLevel.Success, "MySQL Client", $"Connection stablished <Server={this.server} | Port={this.port} | Database={this.database}>.", username);
                NotifyChange("connected");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Error connecting MySQL Database. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                _connection = null;
                NotifyChange("disconnected");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from a MySQL Server (Database)
        /// </summary>
        /// <returns>(bool *async) Succeded / Failed</returns>
        public async Task<bool> Disconnect()
        {
            // Already disconnected
            if (!this.connected || _connection == null)
            {
                Logger.Add(LogLevel.Warning, "MySQL Client", $"MySQL Client already disconnected.", username);
                return true;
            }

            // Execute Disconnection
            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
                Logger.Add(LogLevel.Info, "MySQL Client", $"Connection Disconnected <Server={this.server} | Port={this.port} | Database={this.database}>.", username);
                NotifyChange("disconnected");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Error closing connection MySQL Database. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                NotifyChange("undefined");
                return false;
            }
        }

        #endregion

        #region Queries Wrappers

        // NonQuery = to insert, update, and delete data.
        // Reader = to query the database
        // Scalar = to return a single value.

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
                Logger.Add(LogLevel.Fatal, "MySQL Client", $"{lastError}", username);
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "MySQL Client", $"(Non)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms. Rows affected: {affectedRows}.", username);
                return affectedRows;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Non)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
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
                Logger.Add(LogLevel.Fatal, "MySQL Client", $"{lastError}", username); ;
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                DbDataReader? reader = await cmd.ExecuteReaderAsync();
                //reader.Close();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "MySQL Client", $"(Reader)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms. Rows affected: {reader.RecordsAffected}.", username);
                return reader;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Reader)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
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
                Logger.Add(LogLevel.Fatal, "MySQL Client", $"{lastError}", username);
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                object? result = await cmd.ExecuteScalarAsync();
                //reader.Close();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "MySQL Client", $"(Scalar)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms.", username);
                return result;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Scalar)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "MySQL Client", $"{lastError}", username);
                return null;
            }
        }


        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on Light MySQL Client.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType)
        {
            this.connected = (_connection != null) ? (_connection.State == ConnectionState.Open) : false;
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new MySQLClientEvent(eventType));
            }
        }

        #endregion

    }
}
