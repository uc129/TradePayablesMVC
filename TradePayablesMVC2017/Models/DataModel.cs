using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace TradePayablesMVC2017.Models
{
    public class DataModel
    {
        private static readonly string connString = "Data Source=LTEH-DB-U-01;Initial Catalog=BankGauranteeFandAUAT;Persist Security Info=True;User ID=BankGuarantee_UAT_read;Password=BankGuarantee@UAT8909";
        private static readonly string po_data_connString = "Data Source=SQLAPP;Initial Catalog=Lnt_PO_Data;Persist Security Info=True;User ID=5quarter;Password=Lteh@2023";
        private static readonly string po_credit_connString = "Data Source=SQLAPP;Initial Catalog=LTHE_Invoice_Tracking;Persist Security Info=True;User ID=5quarter;Password=Lteh@2023";
        private static readonly string vendor_connString = "Data Source=SQLAPP;Initial Catalog=Lnt_PO_Data;Persist Security Info=True;User ID=5quarter;Password=Lteh@2023";
        public static Dictionary<int, string> AgeingGroup = new Dictionary<int, string>
            {
                { 1, "Upto 1 year" },
                { 2, "1-2 year" },
                { 3, "2-3 year" },
                { 4, "More than 3 year" }
            };
     

        public static DataSet GetRawRecords()
        {
            string query = "SELECT DISTINCT * FROM [TradePayablesDataTable] WHERE GL_Account IS NOT NULL";
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
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex);
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
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex);
                return null;
            }

        }

        public static DataSet GetPOCreditPeriodRecords()
        {
            string po_cp_query = "SELECT  [PurchasingDoc] ,MAX([CreditPeriod]) AS CreditPeriod FROM [Lnt_PO_Data].[dbo].[POTemsfromSAP] GROUP BY [PurchasingDoc]";
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
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex);
                throw ex;
                //return null;
            }

        }

        public static DataSet GetVendorRecords()
        {
            //string po_cp_query = "SELECT [pkc_vendor_code],[ZTERM],[industry_type] FROM [Lnt_PO_Data].[dbo].[m_Vendor]";
            string vendor_query = "SELECT [pkc_vendor_code],MAX([ZTERM]),MAX([industry_type]) FROM [Lnt_PO_Data].[dbo].[m_Vendor] WHERE [pkc_vendor_code] ='43206' GROUP BY [pkc_vendor_code]";

            DataSet vendor_ds = new DataSet();

            try
            {
                // Establish a connection to the database


                using (SqlConnection vendor_conection = new SqlConnection(po_credit_connString))
                {
                    vendor_conection.Open();
                    SqlDataAdapter po_da = new SqlDataAdapter(vendor_query, vendor_conection);
                    po_da.Fill(vendor_ds);
                }

                if (vendor_ds.Tables.Count > 0 && vendor_ds.Tables[0].Rows.Count > 0)
                {
                    return vendor_ds;
                }

                else
                    return null;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex);
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
            DataTable processedData;
            // 1. Process Vendor Column
            DataTable vendor_processed = ProcessVendorColumn(myDataTable);
            // 2. Process Purchasing Document Column
            DataTable PoProcessed = ProcessPurchasingDocumentColumn(vendor_processed);

            processedData = PoProcessed;

            foreach (DataRow row in processedData.Rows)
            {

                row.SetNullStringIfEmpty("Purchasing_Document");
                row.SetNullStringIfEmpty("Document_Header");
                row.SetNullStringIfEmpty("Assignment");
                row.SetNullStringIfEmpty("Invoice_Reference");
                row.SetNullStringIfEmpty("Vendor");
                row.SetNullStringIfEmpty("Invoice_Description");
                row.SetNullStringIfEmpty("Vendor_Description");
                row.SetNullStringIfEmpty("GL_Account");
                row.SetNullStringIfEmpty("GL_Description");
                row.SetNullStringIfEmpty("Company_Code");
                row.SetNullStringIfEmpty("Document_Type");
                row.SetNullStringIfEmpty("Document_Number");
                row.SetNullStringIfEmpty("Industry");
                row.SetNullStringIfEmpty("Payment_Terms");
                row.SetNullStringIfEmpty("Profit_Center");

            }

            return processedData;
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
                    string text = row["Invoice_Description"]?.ToString();
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
                if (row["Purchasing_Document"] == DBNull.Value || string.IsNullOrEmpty(row["Purchasing_Document"]?.ToString()))
                {
                    string extractedValue = null;
                    string processColumn = "";

                    // Try to extract from Document Header Text
                    string docHeaderText = row["Document_Header"]?.ToString();
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

                    // If not found, try to extract from Assignment
                    if (string.IsNullOrEmpty(extractedValue))
                    {
                        string invoice_reference = row["Invoice_Reference"]?.ToString();
                        if (!string.IsNullOrEmpty(invoice_reference))
                        {
                            Match match = poRegex.Match(invoice_reference);
                            if (match.Success)
                            {
                                extractedValue = match.Value;
                                processColumn = "Processed From Invoice Reference Column";

                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(extractedValue))
                    {
                        row["Purchasing_Document"] = extractedValue; // Update existing Purchasing Document column
                        if (row["Processed"].ToString() == "Processed from Text Column")
                        {
                            row["Processed"] = processColumn + "and Text Column for VC";
                        }
                        else row["Processed"] = processColumn;
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
            if (!originalTable.Columns.Contains("Purchasing_Document") || !originalTable.Columns.Contains("Vendor"))
            {
                throw new ArgumentException("DataTable must contain 'Purchasing Document' and 'Vendor' columns.", nameof(originalTable));
            }

            // Use LINQ to DataSet to filter the rows
            var filteredRows = from row in originalTable.AsEnumerable()
                               where (row.IsNull("Purchasing_Document") || string.IsNullOrWhiteSpace(row.Field<string>("Purchasing_Document"))) ||
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
                         on table1["Purchasing_Document"] equals table2["PurchasingDoc"] into temp
                         from table2 in temp.DefaultIfEmpty()
                         select new
                         {
                             Column1 = table1["Purchasing_Document"],
                             Column2 = table1["Vendor"],
                             Column3 = table1["Document_Date"],
                             Column4 = table1["Posting_Date"],
                             Column5 = table1["Payment_Date"],
                             Column6 = table1["Amount_Local"],
                             Column7 = table1["Industry"],
                             Column8 = table1["Payment_Terms"],
                             Column9 = table2?["CreditPeriod"] ?? DBNull.Value,
                             Column10 = table1["Invoice_Key"],
                             Column11 = table1["Processed"],
                             Column12 = table1["Edited"],
                             Column13 = table1["GL_Account"],
                             Column14 = table1["GL_Description"],
                             Column15= table1["Company_Code"],
                             Column16 = table1["Document_Type"],
                             Column17=table1["SOURCE"],
                             Column18 = table1["Document_Number"]


                         };

            // Create a new DataTable to hold the joined results
            DataTable joinedTable = new DataTable("JoinedData");

            // Define columns for the new DataTable based on your anonymous type
            joinedTable.Columns.Add("Purchasing_Document", typeof(string));
            joinedTable.Columns.Add("Vendor", typeof(string));
            joinedTable.Columns.Add("Document_Date", typeof(string));
            joinedTable.Columns.Add("Posting_Date", typeof(string));
            joinedTable.Columns.Add("Payment_Date", typeof(string));
            joinedTable.Columns.Add("Amount_Local", typeof(decimal));
            joinedTable.Columns.Add("Industry", typeof(string));
            joinedTable.Columns.Add("Payment_Terms", typeof(string));
            joinedTable.Columns.Add("Credit_Period", typeof(string));
            joinedTable.Columns.Add("Invoice_Key", typeof(string));
            joinedTable.Columns.Add("Processed", typeof(string));
            joinedTable.Columns.Add("Edited", typeof(string));
            joinedTable.Columns.Add("GL_Account", typeof(string));
            joinedTable.Columns.Add("GL_Description", typeof(string));
            joinedTable.Columns.Add("Company_Code", typeof(string));
            joinedTable.Columns.Add("Document_Type", typeof(string));
            joinedTable.Columns.Add("SOURCE", typeof(string));
            joinedTable.Columns.Add("Document_Number", typeof(string));



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
                    item.Column9 == DBNull.Value ? null : item.Column9,
                    item.Column10,
                    item.Column11,
                    item.Column12,
                    item.Column13,
                    item.Column14,
                    item.Column15,
                    item.Column16,
                    item.Column17,
                    item.Column18
                );
            }

            if (joinedTable != null)
            {
                return joinedTable;
            }
            else return null;
        }

        public static DataTable JoinTablesPoCreditAndVendor(DataTable data, DataTable PoCredit)
        {
            var result = from table1 in data.AsEnumerable()
                         join table2 in PoCredit.AsEnumerable()
                         on table1["Purchasing_Document"] equals table2["PurchasingDoc"] into temp
                         from table2 in temp.DefaultIfEmpty()
                         select new
                         {
                             Column1 = table1["Purchasing_Document"],
                             Column2 = table1["Vendor"],
                             Column3 = table1["Document_Date"],
                             Column4 = table1["Posting_Date"],
                             Column5 = table1["Payment_Date"],
                             Column6 = table1["Amount_Local"],
                             Column7 = table1["Industry"],
                             Column8 = table1["Payment_Terms"],
                             Column9 = table2?["CreditPeriod"] ?? DBNull.Value,
                             Column10 = table1["Invoice_Key"],
                             Column11 = table1["Processed"],
                             Column12 = table1["Edited"],
                             Column13 = table1["GL_Account"],
                             Column14 = table1["GL_Description"],
                             Column15 = table1["Company_Code"],
                             Column16 = table1["Document_Type"],
                             Column17 = table1["SOURCE"],
                             Column18 = table1["Document_Number"]


                         };

            // Create a new DataTable to hold the joined results
            DataTable joinedTable = new DataTable("JoinedData");

            // Define columns for the new DataTable based on your anonymous type
            joinedTable.Columns.Add("Purchasing_Document", typeof(string));
            joinedTable.Columns.Add("Vendor", typeof(string));
            joinedTable.Columns.Add("Document_Date", typeof(string));
            joinedTable.Columns.Add("Posting_Date", typeof(string));
            joinedTable.Columns.Add("Payment_Date", typeof(string));
            joinedTable.Columns.Add("Amount_Local", typeof(decimal));
            joinedTable.Columns.Add("Industry", typeof(string));
            joinedTable.Columns.Add("Payment_Terms", typeof(string));
            joinedTable.Columns.Add("Credit_Period", typeof(string));
            joinedTable.Columns.Add("Invoice_Key", typeof(string));
            joinedTable.Columns.Add("Processed", typeof(string));
            joinedTable.Columns.Add("Edited", typeof(string));
            joinedTable.Columns.Add("GL_Account", typeof(string));
            joinedTable.Columns.Add("GL_Description", typeof(string));
            joinedTable.Columns.Add("Company_Code", typeof(string));
            joinedTable.Columns.Add("Document_Type", typeof(string));
            joinedTable.Columns.Add("SOURCE", typeof(string));
            joinedTable.Columns.Add("Document_Number", typeof(string));



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
                    item.Column9 == DBNull.Value ? null : item.Column9,
                    item.Column10,
                    item.Column11,
                    item.Column12,
                    item.Column13,
                    item.Column14,
                    item.Column15,
                    item.Column16,
                    item.Column17,
                    item.Column18
                );
            }

            if (joinedTable != null)
            {
                return joinedTable;
            }
            else return null;
        }




        public static bool UpdateDatabase(List<EditInvoicesViewModel> changedInvoices)
        {
            if (changedInvoices == null || changedInvoices.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No invoices to update.");
                return false;
            }



            System.Diagnostics.Debug.WriteLine($"Attempting to update {changedInvoices.Count} invoices...");

            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened successfully.");

                    // SQL UPDATE statement for your table
                    // Replace 'YourTableName' with the actual name of your table where invoice data is stored.
                    // Ensure column names match your database schema.
                    string updateQuery = @"
                    UPDATE [TradePayablesDataTable]
                    SET
                        [Purchasing_Document] = @PurchasingDocument,
                        [Vendor] = @Vendor,
                        [Edited] = @Edited
                    WHERE
                        [Invoice_Key] = @InvoiceKey";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        // Prepare the command to efficiently handle multiple updates
                        command.Parameters.Add("@PurchasingDocument", SqlDbType.NVarChar); // Adjust DataType as needed
                        command.Parameters.Add("@Vendor", SqlDbType.NVarChar);            // Adjust DataType as needed
                        command.Parameters.Add("@InvoiceKey", SqlDbType.NVarChar);        // Adjust DataType as needed
                        command.Parameters.Add("@Edited", SqlDbType.NVarChar);

                        // Loop through each invoice that needs to be updated
                        foreach (var invoice in changedInvoices)
                        {
                            if ((invoice.PurchasingDocument == null && invoice.Vendor == null) || invoice.InvoiceKey == null)
                            {
                                System.Diagnostics.Debug.WriteLine("Error retreiving values");
                                return false;
                            }

                            //System.Diagnostics.Debug.WriteLine("PurchasingDocument", invoice.PurchasingDocument);
                            //System.Diagnostics.Debug.WriteLine("Vendor", invoice.Vendor);
                            //System.Diagnostics.Debug.WriteLine("Invoice Key", invoice.InvoiceKey);
                            //System.Diagnostics.Debug.WriteLine("Edited", invoice.IsEdited);


                            // Set parameter values for the current invoice
                            command.Parameters["@PurchasingDocument"].Value = invoice.PurchasingDocument ?? "null";
                            command.Parameters["@Vendor"].Value = invoice.Vendor ?? "null";
                            command.Parameters["@Edited"].Value = invoice.IsEdited;
                            command.Parameters["@InvoiceKey"].Value = invoice.InvoiceKey;

                            // Execute the update query
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Successfully updated InvoiceKey: {invoice.InvoiceKey}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"No rows updated for InvoiceKey: {invoice.InvoiceKey}. It might not exist or data was unchanged.");
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("All specified invoices processed for update.");
                    return true;
                }
                catch (SqlException ex)
                {
                    // Log specific SQL errors
                    System.Diagnostics.Debug.WriteLine($"Database error occurred: {ex}");
                    System.Diagnostics.Debug.WriteLine($"Error Code: {ex}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return false;
                }
                catch (Exception ex)
                {
                    // Catch any other general exceptions
                    System.Diagnostics.Debug.WriteLine($"An unexpected error occurred: {ex}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return false;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        System.Diagnostics.Debug.WriteLine("Database connection closed.");
                    }
                }
            }
        }

        public static DataTable MSMECreditPeriodFix(DataTable processedData)
        {
            processedData.Columns.Add("CP_Fixed");
            foreach (DataRow row in processedData.Rows)
            {
                string pt = row["Payment_Terms"].ToString();
                string cp = row["Credit_Period"].ToString();
                string ind = row["Industry"].ToString();

                if (string.IsNullOrEmpty(cp) && !string.IsNullOrEmpty(pt) && pt != "NULL"){
                    string lastTwoChars = (pt.Substring((pt.ToString().Length - 2)));

                    if (lastTwoChars == "00")
                        lastTwoChars= "0";

                    cp = lastTwoChars;
                    row["Credit_Period"] = lastTwoChars;
                    row["CP_Fixed"] = "true";

                }

                if ((ind == "1" || ind == "2") && cp == "60")
                {
                    row["Credit_Period"] = "45";
                    row["CP_Fixed"] = "true";
                    //System.Diagnostics.Debug.WriteLine("CP changed to 45 for Industry", row["Industry"], "and CP", row["Credit_Period"]);
                }

                if ((ind == "1" || ind == "2") && pt == "1060")
                {
                    //row["Credit_Period"] = "45";
                    row["CP_Fixed"] = "true";
                    row["Payment_Terms"] = "1045";
                    //System.Diagnostics.Debug.WriteLine("Payment Terms CHanged to 1045 for Industry", row["Industry"], "and Payment Terms", row["Payment_Terms"]);
                }

            }

            return processedData;

        }

        public static DateTime? ParseDateOrNull(object dateValue, string format)
        {
            if (dateValue == null || dateValue == DBNull.Value)
            {
                return null;
            }

            string dateString = dateValue.ToString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            if (DateTime.TryParseExact(dateString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                int year = parsedDate.Year;
                int month = parsedDate.Month;
                int day = parsedDate.Day;
                DateTime finalDate = new DateTime(year, month, day);
                //System.Diagnostics.Debug.WriteLine("Year: " + year + " month: " + month + " day: " + day, " finalDate: " + finalDate+" originalDate: "+dateValue.ToString());
                return finalDate;
            }
            else
            {
                // Optional: Log an error if parsing fails for a non-empty string
                System.Diagnostics.Debug.WriteLine($"Warning: Could not parse '{dateString}' with format '{format}'. Returning null.");
                return null;
            }
        }

        public static DateTime GetLastDateOfCurrentQuarter(DateTime? other_date)
        {
            DateTime currentDate;
            if(other_date.HasValue || other_date != null)
            {
                currentDate = other_date.Value;
            }
            else
            {
                currentDate = DateTime.Now;
            }
            int currentMonth = currentDate.Month;
            int currentYear = currentDate.Year;

            DateTime lastDayOfQuarter;

            if (currentMonth >= 1 && currentMonth <= 3) // Q1: January - March
            {
                lastDayOfQuarter = new DateTime(currentYear, 3, 31);
            }
            else if (currentMonth >= 4 && currentMonth <= 6) // Q2: April - June
            {
                lastDayOfQuarter = new DateTime(currentYear, 6, 30);
            }
            else if (currentMonth >= 7 && currentMonth <= 9) // Q3: July - September
            {
                lastDayOfQuarter = new DateTime(currentYear, 9, 30);
            }
            else // Q4: October - December
            {
                lastDayOfQuarter = new DateTime(currentYear, 12, 31);
            }

            return lastDayOfQuarter;
        }

        public static DataTable AgeingCalculation(DataTable fixedCPData, DateTime current_quarter)
           
        {
            fixedCPData.Columns.Add("Ageing", typeof(string));
            fixedCPData.Columns.Add("Ageing_Years", typeof(string));
            fixedCPData.Columns.Add("Ageing_Group", typeof(string));

          

            System.Diagnostics.Debug.WriteLine("Columns", fixedCPData.Columns);
            string date_format = "dd/MM/yyyy";

            foreach (DataRow row in fixedCPData.Rows)
            {
                int cp = 0;
                string C_P = row["Credit_Period"].ToString();
                TimeSpan ageing;
                float ageing_years;
                string ageing_group;
                DateTime? posting_date = ParseDateOrNull(row["Posting_Date"], date_format);
                DateTime? payment_date = ParseDateOrNull(row["Payment_Date"], date_format);

                if (string.IsNullOrEmpty(C_P) || string.IsNullOrWhiteSpace(C_P))
                    cp = 0; 
                else
                    cp = int.Parse(C_P);
                
                if (payment_date != null && payment_date.HasValue)
                {
                    ageing = current_quarter - payment_date.Value.AddDays(cp);
                    ageing_years = ageing.Days / 365;
                    ageing_group = AgeingGroupClassification(ageing.Days);

                    row["Ageing"] = ageing.Days.ToString();
                    row["Ageing_Years"] = ageing_years.ToString();
                    row["Ageing_Group"] = ageing_group;
                }

                else if (posting_date != null && posting_date.HasValue)
                {
                    ageing = current_quarter - posting_date.Value.AddDays(cp);
                    ageing_years = ageing.Days / 365;
                    ageing_group = AgeingGroupClassification(ageing.Days);

                    row["Ageing"] = ageing.Days.ToString();
                    row["Ageing_Years"] = ageing_years.ToString();
                    row["Ageing_Group"] = ageing_group;
                }
            }

            return fixedCPData;
        }

        public static string AgeingGroupClassification(int ageingDays)
        {
      

            float ageingYears = ageingDays / 365;

            if (ageingYears <= 1)
            {
                return AgeingGroup[1];
            }
            else if (ageingYears > 1 && ageingYears <= 2)
            {
                return AgeingGroup[2];
            }
            else if (ageingYears > 2 && ageingYears <= 3)
            {
                return AgeingGroup[3];
            }
            else if (ageingYears > 3)
            {
                return AgeingGroup[4];
            }

            return null;
        }


        public static DataTable HyperionCodeClassification (DataTable data)
        {
            data.Columns.Add("Hyperion_Code", typeof(string));
            data.Columns.Add("Hyp_Code_Description", typeof(string));
            data.Columns.Add("Due_Status", typeof(string));
            data.Columns.Add("Billed_Status", typeof(string));

            foreach (DataRow row in data.Rows)
            {
                string GL_account = row["GL_Account"].ToString();
                GLHyperionMapper.GLAccountMapping mapping = GLHyperionMapper.ProcessGlAccount(GL_account);
                string hyp_code = mapping.HyperionCode;
                string due = mapping.DueStatus;
                string billed = mapping.BilledStatus;

                row["Due_Status"] = due;
                row["Billed_Status"] = billed;

                if (row["Industry"].ToString() == "1" || row["Industry"].ToString() == "2")
                {
                    row["Hyp_Code_Description"] = "MSMED";

                    if (int.Parse(row["Ageing"].ToString()) <= 45)
                    {
                        row["Hyperion_Code"] = "2D170100";
                    }
                    else if(int.Parse(row["Ageing"].ToString()) > 45)
                    {
                        row["Hyperion_Code"] = "2D170200";
                    }
                }
                else
                {
                    row["Hyperion_Code"] = hyp_code;
                }

            }
            
            return data;
        }

    }
}