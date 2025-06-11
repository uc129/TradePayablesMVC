using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TradePayablesMVC2017.Models
{
    public static class DataTableExtensions
    {
        // Extension method to get distinct rows based on all columns
        public static DataTable GetDistinctRows(this DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return dataTable;
            }

            // Get all column names
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            // Use LINQ to get distinct rows based on all column values
            var distinctRows = dataTable.AsEnumerable()
                .GroupBy(row => new DataRowComparer(row, columnNames)) // Use a custom comparer or directly group by an anonymous type of all column values
                .Select(g => g.First())
                .CopyToDataTable();

            return distinctRows;
        }



        // Extension method to get distinct rows based on specific columns
        public static DataTable GetDistinctRows(this DataTable dataTable, params string[] columnNamesToCompare)
        {
            if (dataTable == null || dataTable.Rows.Count == 0 || columnNamesToCompare == null || !columnNamesToCompare.Any())
            {
                return dataTable;
            }

            // Ensure all specified columns exist in the DataTable
            foreach (string colName in columnNamesToCompare)
            {
                if (!dataTable.Columns.Contains(colName))
                {
                    throw new ArgumentException($"Column '{colName}' not found in the DataTable.");
                }
            }

            // Use LINQ to get distinct rows based on specified column values
            var distinctRows = dataTable.AsEnumerable()
                .GroupBy(row => new DataRowComparer(row, columnNamesToCompare)) // Use custom comparer
                .Select(g => g.First())
                .CopyToDataTable();

            return distinctRows;
        }

        // A custom IEqualityComparer for DataRow to compare values of specified columns
        private class DataRowComparer : IEqualityComparer<DataRow>
        {
            private readonly string[] _columnsToCompare;
            private readonly DataRow _sampleRow; // Used to determine column types for comparison

            public DataRowComparer(DataRow sampleRow, params string[] columnsToCompare)
            {
                _sampleRow = sampleRow;
                _columnsToCompare = columnsToCompare;
            }

            public bool Equals(DataRow x, DataRow y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

                foreach (string colName in _columnsToCompare)
                {
                    if (!object.Equals(x[colName], y[colName]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(DataRow obj)
            {
                if (obj == null) return 0;

                unchecked // Allow overflow
                {
                    int hash = 17; // A prime number
                    foreach (string colName in _columnsToCompare)
                    {
                        object value = obj[colName];
                        hash = hash * 23 + (value?.GetHashCode() ?? 0); // Another prime number
                    }
                    return hash;
                }
            }
        }
    }
}