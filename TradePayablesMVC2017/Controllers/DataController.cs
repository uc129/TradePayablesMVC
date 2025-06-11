using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using TradePayablesMVC2017.Models;

namespace TradePayablesMVC2017.Controllers
{

    public class DataController : Controller
    {


        public static DataSet RawResult = new DataSet();
        public static DataSet PoCreditRecords = new DataSet();
        public static DataTable RawData = new DataTable();
        public static DataTable processedData = new DataTable();
        public static DataTable filteredProcessedData = new DataTable();
        public static DataTable JoinedRawAndPOData = new DataTable();


        public DataController()
        {
            RawResult= DataModel.GetRawRecords(); //get raw records from the TradePayablesDataDump SQL View as a DataSet for ease of manipulation
            RawData = RawResult.Tables[0]; // convert DataSet into a DataTable

            if(RawData != null)
            {
                processedData = DataModel.ProcessDataTable(RawData); //Populate the Vendor Codes and PO Numbers from required fields

            }

            PoCreditRecords = DataModel.GetPOCreditPeriodRecords(); // get PO Credit Period Records

            if (PoCreditRecords != null && RawData != null)
            {
                JoinedRawAndPOData = DataModel.JoinTablesRawAndPOCredit(RawData,PoCreditRecords.Tables[0]); //Join Processed Data with PO Credit Period Records
            }


        }

        //GET Data/Index
        public ActionResult Index()
        {

            if (RawData == null)
            {
                return View();
            }

            List<RawInvoicesViewModel> rawInvoices = new List<RawInvoicesViewModel>();

            foreach (DataRow row in RawData.Rows)
            {
                rawInvoices.Add(new RawInvoicesViewModel
                {
                    PurchasingDocument = row["Purchasing Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Text = row["Text"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment terms"]?.ToString(),
                    DocumentHeaderText = row["Document Header Text"]?.ToString(),
                    AmountLocal = row["Amount_Local"]?.ToString(),
                    Assignment = row["Assignment"]?.ToString(),
                    DocumentDate = row["Document Date"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting Date"].ToString(),
                });
            }

            var viewModel = new RawInvoicesListModel { Invoices = rawInvoices };
            return View(viewModel);

        }

        public ActionResult ProcessedInvoices()
        {

            if (processedData == null)
            {
                return View();
            }

            List<ProcessedInvoicesViewModel> processedInvoices = new List<ProcessedInvoicesViewModel>();

            foreach (DataRow row in processedData.Rows)
            {
                processedInvoices.Add(new ProcessedInvoicesViewModel
                {
                    PurchasingDocument = row["Purchasing Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Text = row["Text"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment terms"]?.ToString(),
                    DocumentHeaderText = row["Document Header Text"]?.ToString(),
                    AmountLocal = row["Amount_Local"]?.ToString(),
                    Assignment = row["Assignment"]?.ToString(),
                    DocumentDate = row["Document Date"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting Date"].ToString(),
                    Processed = row["Processed"].ToString()

                });
            }

            var viewModel = new ProcessedInvoicesListModel { Invoices = processedInvoices };
            return View(viewModel);
        }

        //GET Data/EditData
        public ActionResult EditData()
        {

            // Convert DataTable to List<PurchaseOrderViewModel>
            List<EditInvoicesViewModel> invoices = new List<EditInvoicesViewModel>();
            int idCounter = 1; // Assign a unique ID for each row for client-side tracking
            foreach (DataRow row in processedData.Rows)
            {
                invoices.Add(new EditInvoicesViewModel
                {
                    id = idCounter++,
                    PurchasingDocument = row["Purchasing Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Text = row["Text"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment terms"]?.ToString(),
                    DocumentHeaderText = row["Document Header Text"]?.ToString(),
                    Assignment= row["Assignment"]?.ToString(),
                    IsEdited = false // Initialize as not edited
                });
            }

            var viewModel = new EditInvoicesListModel { Invoices = invoices };
            return View(viewModel);
        }

        // This POST action will receive the edited data
        [HttpPost]
        public ActionResult SaveEditedData(List<EditInvoicesViewModel> editedOrders)
        {
            if (editedOrders == null)
            {
                return Json(new { success = false, message = "No data received." });
            }

            // --- IMPORTANT: Process only truly edited rows ---
            var changedOrders = editedOrders.Where(o => o.IsEdited).ToList();

            // In a real application, you would:
            // 1. Fetch the original data for these IDs from your database/source.
            // 2. Apply the changes from changedOrders to your underlying data store.
            // 3. Perform any server-side validation.



            System.Diagnostics.Debug.WriteLine($"Received {editedOrders.Count} rows, {changedOrders.Count} were marked as edited.");

            foreach (var order in changedOrders)
            {
                // Here you would typically update your database record
                // For now, just print to console/debug output
                System.Diagnostics.Debug.WriteLine($"Saving edited row ID: {order.id}");
                System.Diagnostics.Debug.WriteLine($"   Purchasing Document: {order.PurchasingDocument}");
                System.Diagnostics.Debug.WriteLine($"   Vendor: {order.Vendor}");
                System.Diagnostics.Debug.WriteLine($"   Payment Terms: {order.PaymentTerms}");
                // ... and so on for other fields
                // You might also perform server-side cleanup/defaulting here:
                // order.Vendor = string.IsNullOrWhiteSpace(order.Vendor) ? "N/A" : order.Vendor;
            }

            // After processing, you might return a success message or updated data
            return Json(new { success = true, message = $"{changedOrders.Count} rows processed successfully!" });
        }


        //GET Data/JoinedPOData
        public ActionResult JoinedPOData()
        {
            // Convert DataTable to List<PurchaseOrderViewModel>
            List<JoinedRawAndPOInvoicesViewModel> invoices = new List<JoinedRawAndPOInvoicesViewModel>();
            foreach (DataRow row in JoinedRawAndPOData.Rows)
            {
                invoices.Add(new JoinedRawAndPOInvoicesViewModel
                {
                    PurchasingDocument = row["Purchasing Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment terms"]?.ToString(),
                    AmountLocal = row["Amount Local"]?.ToString(),
                    DocumentDate = row["Document Date"].ToString(),
                    PaymentDate = row["Payment Date"].ToString(),
                    PostingDate = row["Posting Date"].ToString(),
                    CreditPeriod = row["Credit Period"].ToString()
                });
            }

            var viewModel = new JoinedRawAndPOInvoicesListModel { Invoices = invoices };
            return View(viewModel);




            return View();
        }

        


    }
}
