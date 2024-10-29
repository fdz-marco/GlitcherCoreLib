using System.Text.Json;

namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class/Object Definition) Query Parser Structure: Select
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.14 - July 14, 2024
    /// </remarks>

    public class DBSelect
    {

        #region Properties

        public List<string> fields { get; set; } = new List<string>();
        public List<string> datatypes { get; set; } = new List<string>();
        public List<List<string>> values { get; set; } = new List<List<string>>();
        public DBMetadata? metadata { get; set; } = null;

        #endregion

        #region Methods

        /// <summary>
        /// Get Fields Names as Text
        /// </summary>
        /// <param name="separator">Separator (Default: ";")</param>
        /// <returns>(string) String with Fields Names</returns>
        public string GetFieldsAsText(string separator = ";")
        {
            return string.Join(separator, fields);
        }

        /// <summary>
        /// Get DataTypes as Text
        /// </summary>
        /// <param name="separator">Separator (Default: ";")</param>
        /// <returns>(string) String with DataTypes Names</returns>
        public string GetDatatypesAsText(string separator = ";")
        {
            return string.Join(separator, datatypes);
        }

        /// <summary>
        /// Get Values as Text
        /// </summary>
        /// <param name="separator">Separator (Default: ";")</param>
        /// <returns>(string) String with Values (all rows)</returns>
        public string GetValuesAsText(string separator = ";")
        {
            string text = "";
            foreach (List<string> row in values)
            {
                text += string.Join(separator, row) + "\n";
            }
            return text;
        }

        /// <summary>
        /// Get Row by Index
        /// </summary>
        /// <param name="index">Index of Row</param>
        /// <returns>(List<string>) List of strings with values *Row</returns>
        public List<string> GetRow(int index)
        {
            return values[index];
        }

        /// <summary>
        /// Get Row by Index as Text
        /// </summary>
        /// <param name="index">Index of Row</param>
        /// <param name="separator">Separator (Default: ";")</param>
        /// <returns>(string) String with Values of specific row</returns>
        public string GetRowAsText(int index, string separator = ";")
        {
            return string.Join(separator, GetRow(index));
        }

        /// <summary>
        /// Get Column by Index
        /// </summary>
        /// <param name="index">Index of Column</param>
        /// <returns>(List<string>) List of strings with values *Column</returns>
        public List<string> GetColumn(int index)
        {
            List<string> column = new List<string>();
            foreach (List<string> row in values)
            {
                if (row.Count < index)
                    return column;
                column.Add(row[index].ToString());
            }
            return column;
        }

        /// <summary>
        /// Get Column by Index as Text
        /// </summary>
        /// <param name="index">Index of Column</param>
        /// <param name="separator">Separator (Default: ";")</param>
        /// <returns>(string) String with Values of specific column</returns>
        public string GetColumnAsText(int index, string separator = ";")
        {
            return string.Join(separator, GetColumn(index));
        }

        /// <summary>
        /// Convert all fieldnames and values to string (rows+columns)
        /// </summary>
        /// <param name="separator">Separator (Default: ";")</param>
        /// <param name="fieldNames">Include field names or not</param>
        /// <returns>(string) String with Field Names + Values</returns>
        public string ToString(string separator = ";", bool fieldNames = true)
        {
            var text = "";
            if (fieldNames)
                text += GetFieldsAsText(separator) + "\n";
            text += GetValuesAsText(separator);
            return text;
        }

        /// <summary>
        /// Convert all fieldnames, datatypes, metadata and values to string (rows+columns)
        /// </summary>
        /// <param name="fieldNames">Include field names or not</param>
        /// <param name="datatypes">Include data types or not</param>
        /// <param name="metadata">Include metadata or not</param>
        /// <returns>(string) String in JSON Format with Field Names + DataTypes + Values + Metedata</returns>
        public string ToJSON(bool fieldNames = true, bool datatypes = false, bool metadata = true)
        {
            var obj = new
            {
                fields = fields,
                datatypes = this.datatypes,
                values = values,
                metadata = this.metadata
            };
            obj.metadata.SetTotal(Count());
            if (!fieldNames) obj.fields.Clear();
            if (!datatypes) obj.datatypes.Clear();
            if (!metadata) obj.metadata.Clear();

            return JsonSerializer.Serialize(obj); ;
        }

        /// <summary>
        /// Count number of rows
        /// </summary>
        /// <returns>(int) Number of rows</returns>
        public int Count()
        {
            if (values == null)
                return -1;
            return values.Count;
        }

        #endregion

    }
}