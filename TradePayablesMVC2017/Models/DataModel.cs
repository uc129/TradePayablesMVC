using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TradePayablesMVC2017.Models
{
    public class DataModel
    {
        private static readonly string connString = "Data Source=LTEH-DB-U-01;Initial Catalog=BankGuarantee_UAT;Persist Security Info=True;User ID=BankGuarantee_UAT_read;Password=BankGuarantee@UAT8909";
        private static readonly string po_data_connString = "Data Source=SQLAPP;Initial Catalog=Lnt_PO_Data;Persist Security Info=True;User ID=5quarter;Password=Lteh@2023";
        private static readonly string po_credit_connString = "Data Source=SQLAPP;Initial Catalog=LTHE_Invoice_Tracking;Persist Security Info=True;User ID=5quarter;Password=Lteh@2023";
        public static DataSet GetRawRecords()
        {
            string query = "SELECT * FROM [TradePayableDataDump]";
            DataSet raw_ds = new DataSet();
            
            try
            {
                // Establish a connection to the database
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, connection);          
                    da.Fill(raw_ds);
                    connection.Close();
                }

               
                
                if (raw_ds.Tables.Count > 0 && raw_ds.Tables[0].Rows.Count > 0)
                {
                    return raw_ds;
                }
                    
                else
                    return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex);
                return null;
            }

        }

        public static DataSet GetPORecords()
        {
            string po_query = "SELECT DISTINCT [ebeln],[lifnr] FROM [podata]";
            DataSet po_ds = new DataSet();

            try
            {
                // Establish a connection to the database
               

                using (SqlConnection po_header_conection = new SqlConnection(po_data_connString))
                {
                    po_header_conection.Open();
                    SqlDataAdapter po_da = new SqlDataAdapter(po_query, po_header_conection);
                    po_da.Fill(po_ds);
                }

                if (po_ds.Tables.Count > 0 && po_ds.Tables[0].Rows.Count > 0)
                {
                    return po_ds;
                }

                else
                    return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex);
                return null;
            }

        }

        public static DataSet GetPOCreditPeriodRecords()
        {
            string po_cp_query = "SELECT DISTINCT [PurchasingDoc],[CreditPeriod] FROM [POTemsfromSAP]";
            DataSet po_cp_ds = new DataSet();

            try
            {
                // Establish a connection to the database


                using (SqlConnection po_cp_conection = new SqlConnection(po_credit_connString))
                {
                    po_cp_conection.Open();
                    SqlDataAdapter po_da = new SqlDataAdapter(po_cp_query, po_cp_conection);
                    po_da.Fill(po_cp_ds);
                }

                if (po_cp_ds.Tables.Count > 0 && po_cp_ds.Tables[0].Rows.Count > 0)
                {
                    return po_cp_ds;
                }

                else
                    return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex);
                throw ex;
                //return null;
            }

        }

        public static string ConvertDataTableToHTMLString(DataTable dt)
        {
            StringBuilder html = new StringBuilder();

            // Start the table
            html.Append("<table border='1' style='border-collapse: collapse; width: 100%;'>");

            // Add the header row
            html.Append("<tr class='table-rows'>");
            foreach (DataColumn column in dt.Columns)
            {
                html.Append($"<th class='table-header-cells' style='padding: 3px; text-align: left;'>{column.ColumnName}</th>");
            }
            html.Append("</tr>");

            // Add the data rows
            foreach (DataRow row in dt.Rows)
            {
                html.Append("<tr class='table-rows'>");
                foreach (var cell in row.ItemArray)
                {
                    html.Append($"<td class='table-cells' style='padding: 3px;'>{cell}</td>");
                }
                html.Append("</tr>");
            }

            // End the table
            html.Append("</table>");

            return html.ToString();
        }

        public static DataTable ProcessDataTable(DataTable myDataTable)
        {
            // 1. Process Vendor Column
            DataTable vendor_processed = ProcessVendorColumn(myDataTable);
            // 2. Process Purchasing Document Column
            DataTable PoProcessed = ProcessPurchasingDocumentColumn(vendor_processed);  

            return PoProcessed;
        }

        private static DataTable ProcessVendorColumn(DataTable myDataTable)
        {
            // Option A: Update the existing "Vendor" column
            // If you want a new column, uncomment the next lines and modify the loop accordingly.
            // if (!myDataTable.Columns.Contains("ExtractedVendor"))
            // {
            //     myDataTable.Columns.Add("ExtractedVendor", typeof(string));
            // }

            string vendorRegexPattern = @"\bLT\d{4}\b|\bVC-\d{7}\b|\bVC-\d{5}\b";
            Regex vendorRegex = new Regex(vendorRegexPattern, RegexOptions.IgnoreCase);
            myDataTable.Columns.Add("Processed", typeof(string));

            foreach (DataRow row in myDataTable.Rows)
            {
                if (row["Vendor"] == DBNull.Value || string.IsNullOrEmpty(row["Vendor"]?.ToString()))
                {
                    string text = row["Text"]?.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        Match match = vendorRegex.Match(text);
                        if (match.Success)
                        {
                            row["Vendor"] = match.Value; // Update existing Vendor column
                            row["Processed"] = "Processed from Text Column"; // If using a new column: row["ExtractedVendor"] = match.Value;
                        }
                    }
                }
            }
            return myDataTable;
        }

        private static DataTable ProcessPurchasingDocumentColumn(DataTable myDataTable)
        {
            // Option A: Update the existing "Purchasing Document" column
            // If you want a new column, uncomment the next lines and modify the loop accordingly.
            // if (!myDataTable.Columns.Contains("ExtractedPurchasingDocument"))
            // {
            //     myDataTable.Columns.Add("ExtractedPurchasingDocument", typeof(string));
            // }

            string poRegexPattern = @"\b7\d{9}\b|\b8\d{9}\b|\b31\d{8}\b";
            Regex poRegex = new Regex(poRegexPattern);

            foreach (DataRow row in myDataTable.Rows)
            {
                if (row["Purchasing Document"] == DBNull.Value || string.IsNullOrEmpty(row["Purchasing Document"]?.ToString()))
                {
                    string extractedValue = null;
                    string processColumn = "";

                    // Try to extract from Document Header Text
                    string docHeaderText = row["Document Header Text"]?.ToString();
                    if (!string.IsNullOrEmpty(docHeaderText))
                    {
                        Match match = poRegex.Match(docHeaderText);
                        if (match.Success)
                        {
                            extractedValue = match.Value;
                            processColumn = "Processed From Document Header Text";
                        }
                    }

                    // If not found, try to extract from Assignment
                    if (string.IsNullOrEmpty(extractedValue))
                    {
                        string assignment = row["Assignment"]?.ToString();
                        if (!string.IsNullOrEmpty(assignment))
                        {
                            Match match = poRegex.Match(assignment);
                            if (match.Success)
                            {
                                extractedValue = match.Value;
                                processColumn = "Processed From Assignment Column";

                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(extractedValue))
                    {
                        row["Purchasing Document"] = extractedValue; // Update existing Purchasing Document column
                        row["Processed"] = processColumn;            // If using a new column: row["ExtractedPurchasingDocument"] = extractedValue;
                    }
                }
            }
            return myDataTable;


        }

        public static DataTable FilterNullPurchasingDocumentOrVendor(DataTable originalTable)
        {
            if (originalTable == null)
            {
                throw new ArgumentNullException(nameof(originalTable), "Original DataTable cannot be null.");
            }

            // Check if the columns exist before attempting to access them
            if (!originalTable.Columns.Contains("Purchasing Document") || !originalTable.Columns.Contains("Vendor"))
            {
                throw new ArgumentException("DataTable must contain 'Purchasing Document' and 'Vendor' columns.", nameof(originalTable));
            }

            // Use LINQ to DataSet to filter the rows
            var filteredRows = from row in originalTable.AsEnumerable()
                               where (row.IsNull("Purchasing Document") || string.IsNullOrWhiteSpace(row.Field<string>("Purchasing Document"))) ||
                                (row.IsNull("Vendor") || string.IsNullOrWhiteSpace(row.Field<string>("Vendor")))
                               select row;

            // Create a new DataTable from the filtered rows
            // If no rows match the filter, CopyToDataTable() will throw an error.
            // So, we check if there are any rows before calling CopyToDataTable().
            if (filteredRows.Any())
            {
                return filteredRows.CopyToDataTable();
            }
            else
            {
                // If no rows match, return an empty DataTable with the same schema
                return originalTable.Clone();
            }
        }

        public static DataTable JoinTablesRawAndPOCredit(DataTable processedData, DataTable PoCredit)
        {
            var result = from table1 in processedData.AsEnumerable()
                         join table2 in PoCredit.AsEnumerable()
                         on table1["Purchasing Document"] equals table2["PurchasingDoc"] into temp
                         from table2 in temp.DefaultIfEmpty()
                         select new
                         {
                             Column1 = table1["Purchasing Document"],
                             Column2 = table1["Vendor"],
                             Column3 = table1["Document Date"],
                             Column4 = table1["Posting Date"],
                             Column5 = table1["Payment_Date"],
                             Column6 = table1["Amount_Local"],
                             Column7 = table1["Industry"],
                             Column8 = table1["Payment terms"],
                             Column9 = table2?["CreditPeriod"] ?? DBNull.Value
                         };

            // Create a new DataTable to hold the joined results
            DataTable joinedTable = new DataTable("JoinedData");

            // Define columns for the new DataTable based on your anonymous type
            joinedTable.Columns.Add("Purchasing Document", typeof(string));
            joinedTable.Columns.Add("Vendor", typeof(string));
            joinedTable.Columns.Add("Document Date", typeof(DateTime));
            joinedTable.Columns.Add("Posting Date", typeof(DateTime));
            joinedTable.Columns.Add("Payment Date", typeof(DateTime));
            joinedTable.Columns.Add("Amount Local", typeof(decimal));
            joinedTable.Columns.Add("Industry", typeof(string));
            joinedTable.Columns.Add("Payment terms", typeof(string));
            joinedTable.Columns.Add("Credit Period", typeof(string)); // Renamed for clarity, was Column11

            // Populate the new DataTable with data from the LINQ result
            foreach (var item in result)
            {
                joinedTable.Rows.Add(
                    item.Column1,
                    item.Column2,
                    item.Column3,
                    item.Column4,
                    item.Column5,
                    item.Column6,
                    item.Column7,
                    item.Column8,
                    item.Column9 == DBNull.Value ? null : item.Column9
                );
            }

            if (joinedTable != null)
            {
                return joinedTable;
            }
            else return null;
        }

        public static DataTable JoinTablesRawAndPO(DataTable processedData, DataTable PoData)
        {
            var result = from table1 in processedData.AsEnumerable()
                         join table2 in PoData.AsEnumerable()
                         on table1["Purchasing Document"] equals table2["ebeln"] into temp
                         from table2 in temp.DefaultIfEmpty()
                         select new
                         {
                             Column1 = table1["Purchasing Document"],
                             Column2 = table1["Vendor"],
                             Column3 = table1["Document Date"],
                             Column4 = table1["Posting Date"],
                             Column5 = table1["Amount_Local"],
                             Column6 = table1["Text"],
                             Column7 = table1["Industry"],
                             Column8 = table1["Payment terms"],
                             Column9 = table1["Document Header Text"],
                             Column10 = table2?["ebeln"]?? DBNull.Value,
                             Column11 = table2?["lifnr"]?? DBNull.Value
                         };

            // Create a new DataTable to hold the joined results
            DataTable joinedTable = new DataTable("JoinedData");

            // Define columns for the new DataTable based on your anonymous type
            joinedTable.Columns.Add("Purchasing Document", typeof(string));
            joinedTable.Columns.Add("Vendor", typeof(string));
            joinedTable.Columns.Add("Document Date", typeof(DateTime));
            joinedTable.Columns.Add("Posting Date", typeof(DateTime));
            joinedTable.Columns.Add("Amount_Local", typeof(decimal));
            joinedTable.Columns.Add("Text", typeof(string));
            joinedTable.Columns.Add("Industry", typeof(string));
            joinedTable.Columns.Add("Payment terms", typeof(string));
            joinedTable.Columns.Add("Document Header Text", typeof(string));
            joinedTable.Columns.Add("PO_ebeln", typeof(string)); // Renamed for clarity, was Column10
            joinedTable.Columns.Add("PO_lifnr", typeof(string)); // Renamed for clarity, was Column11

            // Populate the new DataTable with data from the LINQ result
            foreach (var item in result)
            {
                joinedTable.Rows.Add(
                    item.Column1,
                    item.Column2,
                    item.Column3,
                    item.Column4,
                    item.Column5,
                    item.Column6,
                    item.Column7,
                    item.Column8,
                    item.Column9,
                    item.Column10 == DBNull.Value ? null : item.Column10, // Handle DBNull for string/object
                    item.Column11 == DBNull.Value ? null : item.Column11  // Handle DBNull for string/object
                );
            }

            if (joinedTable != null)
            {
                return joinedTable;
            }
            else return null;
        }


        public static DataTable UpdateDatabase(DataTable updatedData)
        {
            DataTable updated = new DataTable();
            return updated;
        }
    }
}