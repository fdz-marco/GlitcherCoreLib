using System.Data;
using System.Data.Common;

namespace glitcher.core.Databases
{
    /// <summary>
    /// (Class/Object Definition) Query Parser<br/>
    /// Parse the queries to handle better query results.
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.14 - July 14, 2024
    /// </remarks>
    public static class QueryParser
    {

        /// <summary>
        /// Parse a Select Query (ReaderQueryAsync)
        /// </summary>
        /// <param name="reader">Reader Result</param>
        /// <param name="metadata">Metadata</param>
        /// <returns>(DBSelect *async) Object with the Results</returns>
        public static async Task<DBSelect?> Select(DbDataReader? reader, DBMetadata? metadata = null)
        {
            if (reader == null)
            {
                Logger.Add(LogLevel.Fatal, "Query Parser", $"Null Reader.");
                //MessageBox.Show($"Null Reader.", "Fatal Error - Query Parser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (reader.HasRows)
            {
                return await Task.Run(async () =>
                {
                    // Create Parser Object
                    DBSelect select = new DBSelect();
                    // Number of Params
                    int paramsNum = reader.FieldCount;
                    // Get Fields + Types
                    DataTable? schema = await reader.GetSchemaTableAsync();
                    if (schema != null)
                    {
                        foreach (DataRow field in schema.Rows)
                        {
                            if (field != null)
                            {
                                string columnName = field[schema.Columns["ColumnName"]].ToString();
                                string dataType = field[schema.Columns["DataType"]].ToString();
                                select.fields.Add(columnName);
                                select.datatypes.Add(dataType);
                            }
                        }
                    }
                    // Get Values
                    while (reader.Read())
                    {
                        List<string> row = new List<string>();
                        for (int i = 0; i < paramsNum; i++)
                        {
                            row.Add(reader[i].ToString());
                        }
                        select.values.Add(row);
                    }
                    // Add Metadata
                    if (metadata != null)
                    {
                        select.metadata = metadata;
                    }
                    // Close Reader
                    await reader.CloseAsync();
                    // Return Select
                    return select;
                });
            }
            return null;
        }
    }
}