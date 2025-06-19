using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace TradePayablesMVC2017.Models
{

    public static class DataRowExtensions
    {
        /// <summary>
        /// Checks if a specified column in a DataRow is null, empty, or whitespace,
        /// and if so, sets its value to "NULL".
        /// </summary>
        /// <param name="row">The DataRow to process.</param>
        /// <param name="columnName">The name of the column to check.</param>
        public static void SetNullStringIfEmpty(this DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName))
            {
                string columnValue = row[columnName].ToString();
                if (string.IsNullOrEmpty(columnValue) || string.IsNullOrWhiteSpace(columnValue) || columnValue == "null")
                {
                    row[columnName] = "null";
                }
            }
            else
            {
                // Optionally, handle cases where the column doesn't exist.
                // For example, throw an exception or log a warning.
                // throw new ArgumentException($"Column '{columnName}' does not exist in the DataRow.");
            }
        }
    }
}