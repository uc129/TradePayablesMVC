using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TradePayablesMVC2017.Models
{
    public class DataToDB
    {
        public static bool CreateOrOverwriteDataTable(string tableName, DataTable schemaTable, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                System.Diagnostics.Debug.WriteLine("Table name cannot be null or empty.");
                return false;
            }
            if (schemaTable == null || schemaTable.Columns.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Schema table is null or has no columns. Cannot create table.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                System.Diagnostics.Debug.WriteLine("Connection string cannot be null or empty.");
                return false;
            }

            // Build the DROP TABLE IF EXISTS statement
            string dropTableSql = $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE [{tableName}];";

            // Build the CREATE TABLE SQL statement
            string createTableSql = BuildCreateTableStatement(tableName, schemaTable);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // 1. Execute DROP TABLE IF EXISTS
                    using (SqlCommand dropCommand = new SqlCommand(dropTableSql, connection))
                    {
                        dropCommand.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Attempted to drop table '{tableName}' if it existed.");
                    }

                    // 2. Execute CREATE TABLE
                    using (SqlCommand createCommand = new SqlCommand(createTableSql, connection))
                    {
                        createCommand.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Table '{tableName}' created successfully.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"An error occurred during table creation/overwrite: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine(ex.ToString()); // Print full exception details
                    return false;
                }
            }
        }

        // Helper method to build the CREATE TABLE SQL statement dynamically
        private static string BuildCreateTableStatement(string tableName, DataTable schemaTable)
        {
            var columns = new StringBuilder();

            foreach (DataColumn column in schemaTable.Columns)
            {
                string sqlDataType = GetSqlDataType(column.DataType);
                string nullable = column.AllowDBNull ? "NULL" : "NOT NULL";

                if (columns.Length > 0)
                {
                    columns.Append(", ");
                }
                columns.Append($"[{column.ColumnName}] {sqlDataType} {nullable}");
            }

            return $"CREATE TABLE [{tableName}] ({columns.ToString()});";
        }

        // Helper method to map C# data types to SQL data types
        private static string GetSqlDataType(Type dataType)
        {
            if (dataType == typeof(string)) return "NVARCHAR(MAX)";
            else if (dataType == typeof(int)) return "INT";
            else if (dataType == typeof(long)) return "BIGINT";
            else if (dataType == typeof(decimal)) return "DECIMAL(18, 2)";
            else if (dataType == typeof(double)) return "FLOAT";
            else if (dataType == typeof(float)) return "REAL";
            else if (dataType == typeof(bool)) return "BIT";
            else if (dataType == typeof(DateTime)) return "DATETIME";
            else if (dataType == typeof(Guid)) return "UNIQUEIDENTIFIER";
            else if (dataType == typeof(byte[])) return "VARBINARY(MAX)";
            else
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Unhandled data type '{dataType.Name}'. Defaulting to NVARCHAR(MAX).");
                return "NVARCHAR(MAX)";
            }


        }

        public static void SaveDataTableToSql(DataTable sourceTable, string tableNameInDb, string connectionString)
        {
            if (sourceTable == null || sourceTable.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("The source DataTable is empty or null. No data to save.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableNameInDb;

                    // Optional: Map columns if your DataTable column names differ from the SQL table
                    // If column names are identical and in the same order, this isn't strictly necessary,
                    // but it's good practice for robustness.
                    foreach (DataColumn column in sourceTable.Columns)
                    {
                        // Assuming DataTable column name matches SQL table column name
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    // If you have specific mappings:
                    // bulkCopy.ColumnMappings.Add("Hyperion_Code", "Database_Hyperion_Code_Column");
                    // bulkCopy.ColumnMappings.Add("Amount_Local", "Database_Amount_Column");
                    // ... and so on for all columns you want to transfer

                    try
                    {
                        bulkCopy.WriteToServer(sourceTable);
                        System.Diagnostics.Debug.WriteLine($"Successfully saved {sourceTable.Rows.Count} rows to {tableNameInDb}.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving data to SQL database: {ex.Message}");
                        // Log the full exception details
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }

    }
}