namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class/Object Definition) Database Metada
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.14 - July 14, 2024
    /// </remarks>

    public class DBMetadata
    {

        #region Properties

        public string driver { get; set; } = string.Empty;
        public string source { get; set; } = string.Empty;
        public string query { get; set; } = string.Empty;
        public string dateExecuted { get; set; } = string.Empty;
        public int total { get; set; } = 0;
        public Dictionary<string, string> terms { get; set; } = new Dictionary<string, string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Create an Object to store Metadata of Query
        /// </summary>
        /// <param name="driver">Database Driver</param>
        /// <param name="source">Database Source</param>
        /// <param name="query">Query Executed</param>
        /// <param name="terms">Dictionary of Term (extra Metadata)</param>
        /// <returns>(void)</returns>
        public DBMetadata(string? driver = null, string? source = null, string? query = null, Dictionary<string, string>? terms = null)
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            dateExecuted = formattedDateTime;

            if (driver != null) { this.driver = driver; }
            if (source != null) { this.source = source; }
            if (query != null) { this.query = query; }
            if (terms != null) { this.terms = terms; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear All Metadata
        /// </summary>
        /// <returns>(void)</returns>
        public void Clear()
        {
            driver = string.Empty;
            source = string.Empty;
            query = string.Empty;
            dateExecuted = string.Empty;
            terms.Clear();
        }

        /// <summary>
        /// Add Term (To Metadata)
        /// </summary>
        /// <param name="term">Term</param>
        /// <param name="value">Value</param>
        /// <returns>(void)</returns>
        public void AddTerm(string term, string value)
        {
            terms.Add(term, value);
        }

        /// <summary>
        /// Set Total
        /// </summary>
        /// <param name="total">Total</param>
        /// <returns>(void)</returns>
        public void SetTotal(int total)
        {
            this.total = total;
        }

        #endregion

    }
}