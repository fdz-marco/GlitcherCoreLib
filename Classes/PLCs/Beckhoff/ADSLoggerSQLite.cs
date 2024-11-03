using System.Text.RegularExpressions;
using glitcher.core;
using Databases = glitcher.core.Databases;


namespace glitcher.core.PLC.Beckhoff
{
    /// <summary>
    /// (Class) TwinCAT ADS Client - SQLite Logger <br/>
    /// Class to log ADS Variables into a SQLite Database file.<br/><br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.04.26 - April 04, 2024
    /// </remarks>
    public class ADSLoggerSQLite
    {

        #region Properties

        private Databases.SQLiteClient? _sqLiteClient = null;

        public string tagPath { get; } = String.Empty;

        #endregion

        #region Constructor / Settings / Initialization Tasks

        /// <summary>
        /// Creates a ADS Client - SQLite Logger
        /// </summary>
        /// <param name="tagPath">Tag Path</param>
        /// <param name="path">Log Folder Path</param>
        public ADSLoggerSQLite(string tagPath, string path = "logs")
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string combinedPath = Path.GetFullPath(Path.Combine(currentPath, path));

            if (!Directory.Exists(combinedPath))
            {
                DirectoryInfo dir = Directory.CreateDirectory(path);
                Logger.Add(LogLevel.Info, "ADS Logger SQLite", $"Directory Created: {dir.FullName}.");
            }

            if ((tagPath != null) || (!String.IsNullOrEmpty(tagPath)))
            {
                this.tagPath = tagPath;
                string fullPathDB = Path.GetFullPath(Path.Combine(combinedPath, $"{this.tagPath}.db"));
                _sqLiteClient = new Databases.SQLiteClient(fullPathDB);
                this.CreateTable();
                Logger.Add(LogLevel.OnlyDebug, "ADS Logger SQLite", $"Log created. <{fullPathDB}>.");
            }
            else
            {
                Logger.Add(LogLevel.Fatal, "ADS Logger SQLite", $"Log creation failed. Error: Empty tagPath.");
            }
        }

        /// <summary>
        /// Dispose the ADS Client - SQLite Logger Object
        /// </summary>
        public void Dispose()
        {
            if (_sqLiteClient != null) return;
            this._sqLiteClient.Disconnect();
            this._sqLiteClient = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a Table for logging in the SQLite DB
        /// </summary>
        public async Task CreateTable()
        {
            try
            {
                if (_sqLiteClient != null)
                {
                    if (await _sqLiteClient.Connect())
                    {
                        await _sqLiteClient.NonQueryAsync("CREATE TABLE IF NOT EXISTS log (Id INTEGER PRIMARY KEY AUTOINCREMENT, timestamp TEXT, value TEXT)");
                        Logger.Add(LogLevel.Info, "ADS Logger SQLite", $"Success creating log table. Tag: <{tagPath}>.");
                    }
                    else
                    {
                        Logger.Add(LogLevel.Error, "ADS Logger SQLite", $"Error connecting to log database. Tag: <{tagPath}>.");
                    }
                }
                throw new Exception("SQLite Client Null");
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Fatal, "ADS Logger SQLite", $"Error creating log table. Tag: <{tagPath}>. Exception: {ex.Message}.");
            }
        }

        /// <summary>
        /// Insert a Value in the SQLite DB
        /// </summary>
        public async Task InsertValue(string value, string timestamp)
        {
            try
            {
                if (_sqLiteClient != null)
                {
                    if (await _sqLiteClient.Connect())
                    {
                        await _sqLiteClient.NonQueryAsync($"INSERT INTO log (timestamp, value) VALUES ('{timestamp}', '{Regex.Escape(value)}');");
                        Logger.Add(LogLevel.Info, "ADS Logger SQLite", $"Success inserting log. Tag: <{tagPath}>.");
                    }
                    else
                    {
                        Logger.Add(LogLevel.Error, "ADS Logger SQLite", $"Error connecting to log database. Tag: <{tagPath}>.");
                    }
                }
                throw new Exception("SQLite Client Null");
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Fatal, "ADS Logger SQLite", $"Error inserting log. Tag: <{tagPath}>. Exception: {ex.Message}.");
            }
        }

        #endregion
    
    }
}
