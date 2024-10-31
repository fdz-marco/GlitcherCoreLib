using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class) Light SQLite Client <br/>
    /// Class to execute a SQLite Client.<br/><br/>
    /// **Important**<br/>
    /// Nugget Package Required: System.Data.SQLite<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.18 - July 18, 2024
    /// </remarks>
    public class SQLiteClient
    {
        #region Properties

        private SQLiteConnection? _connection = null;

        public string server { get; set; } = string.Empty;
        public string lastError { get; set; } = string.Empty;
        public long lastExec { get; set; } = 0;
        public bool connected { get; set; } = false;
        public string baseURL { get => string.Format("{0}://{1}{2}", DBTypes.SQLite.ToString().ToLower(), AppContext.BaseDirectory.ToLower().Replace("\\","/"), this.server ); }
        public string clientId = "";

        public event EventHandler<SQLiteClientEvent>? ChangeOccurred;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a Light SQLite Client
        /// </summary>
        /// <param name="server">SQLite Server</param>
        /// <param name="autostart">Start sever on creation</param>
        public SQLiteClient(string server = "", bool autostart = true)
        {
            this.server = server;
            this.clientId = Guid.NewGuid().ToString().Substring(0, 6);
            Logger.Add(LogLevel.OnlyDebug, "SQLite Client", $"SQLite Client created. Base URL: <{baseURL}>.", clientId);
            if (autostart)
                this.Connect();
        }

        /// <summary>
        /// Update settings of SQLite Client
        /// </summary>
        /// <param name="server">SQLite Server</param>
        /// <param name="restart">Restart Server on Update</param>
        /// <returns>(void)</returns>
        public async Task Update(string server = "", bool restart = true)
        {
            if (restart)
                await this.Disconnect();
            this.server = server;
            Logger.Add(LogLevel.OnlyDebug, "SQLite Client", $"SQLite Client created. Base URL: <{baseURL}>.", clientId);
            if (restart)
                await this.Connect();
        }

        /// <summary>
        /// Create Database file if doesn't exist
        /// </summary>
        /// <param name="server">SQLite Server</param>
        /// <returns>(bool) True if created, False alrady exists</returns>
        public bool CreateDB(string server = "database.db")
        {
            if (!File.Exists(server))
            {
                SQLiteConnection.CreateFile(server);
                Logger.Add(LogLevel.OnlyDebug, "SQLite Client", $"Database file created. File: {server}.");
                return true;
            }
            else
            {
                Logger.Add(LogLevel.OnlyDebug, "SQLite Client", $"Database file already exist. File: {server}.");
                return false;
            }
        }

        #endregion

        #region Connect / Disconnect

        /// <summary>
        /// Connect to a SQLite Server (Database)
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
                    Logger.Add(LogLevel.Info, "SQLite Client", $"Connection already stablished. Base URL: <{baseURL}>.", clientId);
                    return true;
                }
                else
                {
                    lastError = $"Error connecting SQLite Database. Connection Status: {_connection.State.ToString()}.";
                    Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
                    return false;
                }
            }

            // Check Connection Variables
            if (string.IsNullOrEmpty(this.server))
            {
                lastError = $"Error: No Server name declared.";
                Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
                return false;
            }

            // Create Database file if doesn't exist
            CreateDB(this.server);

            // Execute Connection
            try
            {
                string connectionString = string.Format("Data Source={0}; Version=3; New=True; Compress=True;", this.server);
                _connection = new SQLiteConnection(connectionString);
                await _connection.OpenAsync();
                lastError = "";
                Logger.Add(LogLevel.Success, "SQLite Client", $"Connection Stablished <{baseURL}>.", clientId);
                NotifyChange("connected");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Error connecting SQLLite Database. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
                _connection = null;
                NotifyChange("disconnected");
                return false;
            }
        }


        /// <summary>
        /// Disconnect from a SQLite Server (Database)
        /// </summary>
        /// <returns>(bool *async) Succeded / Failed</returns>
        public async Task<bool> Disconnect()
        {
            // Already disconnected
            if (!this.connected || _connection == null)
            {
                Logger.Add(LogLevel.Warning, "SQLite Client", $"SQLite Client already disconnected.", clientId);
                return true;
            }

            // Execute Disconnection
            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
                Logger.Add(LogLevel.Info, "SQLite Client", $"Connection Disconnected <{baseURL}>.", clientId);
                NotifyChange("disconnected");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Error closing connection SQLite Database. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
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
                Logger.Add(LogLevel.Fatal, "SQLite Client", $"{lastError}", clientId);
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                SQLiteCommand cmd = new SQLiteCommand(query, _connection);
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "SQLite Client", $"(Non)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms. Rows affected: {affectedRows}.", clientId);
                return affectedRows;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Non)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
                return null;
            }
        }

        /// <summary>
        /// Reader = to query the database: select.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <returns>(DbDataReader *async)</returns>
        public async Task<DbDataReader?> QueryAsync(string query)
        {
            // Check connection
            if (!this.connected || _connection == null)
            {
                this.lastError = $"Error during querying database. No connection found.";
                Logger.Add(LogLevel.Fatal, "SQLite Client", $"{lastError}", clientId); ;
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                SQLiteCommand cmd = new SQLiteCommand(_connection);
                cmd.CommandText = query;
                DbDataReader? reader = await cmd.ExecuteReaderAsync();
                //reader.Close();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "SQLite Client", $"(Reader)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms. Rows affected: {reader.RecordsAffected}.", clientId);
                return reader;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Reader)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
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
                Logger.Add(LogLevel.Fatal, "SQLite Client", $"{lastError}", clientId);
                return null;
            }

            // Execute Query
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                SQLiteCommand cmd = new SQLiteCommand(_connection);
                cmd.CommandText = query;
                object? result = await cmd.ExecuteScalarAsync();
                //reader.Close();
                watch.Stop();
                this.lastError = "";
                this.lastExec = watch.ElapsedMilliseconds;
                Logger.Add(LogLevel.Info, "SQLite Client", $"(Scalar)Query successfully executed. Query: <{query}>. Execution Time: {this.lastExec} ms.", clientId);
                return result;
            }
            catch (Exception ex)
            {
                lastError = $"Error executing (Scalar)Query. Query: <{query}>. Exception: {ex.Message}.";
                Logger.Add(LogLevel.Error, "SQLite Client", $"{lastError}", clientId);
                return null;
            }
        }

        #endregion

        #region Notifiers / Event Handlers

        /// <summary>
        /// Notify a change on Lite SQLite Client.
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyChange(string eventType)
        {
            this.connected = (_connection != null) ? (_connection.State == ConnectionState.Open) : false;
            if (ChangeOccurred != null)
            {
                ChangeOccurred.Invoke(this, new SQLiteClientEvent(eventType));
            }
        }

        #endregion

    }
}
