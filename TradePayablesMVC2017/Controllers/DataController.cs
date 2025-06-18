using System;
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
        public static DataSet VendorRecords = new DataSet();

        public static DataTable RawData = new DataTable();
        public static DataTable processedData = new DataTable();
        public static DataTable filteredProcessedData = new DataTable();

        public static DataTable JoinedRawAndPOData = new DataTable();
        public static DataTable JoinedPoDataAndVendor = new DataTable();

        public static DataTable FixedCPData = new DataTable();
        public static DataTable DataWithAgeing = new DataTable();
        public static DataTable DatasWithHyp = new DataTable();


        public static DateTime currentQuarter;


        public DataController()
        {
            RawResult= DataModel.GetRawRecords(); //get raw records from the TradePayablesDataDump SQL View as a DataSet for ease of manipulation
            RawData = RawResult.Tables[0]; // convert DataSet into a DataTable

            if(RawData != null)
            {
                processedData = DataModel.ProcessDataTable(RawData); //Populate the Vendor Codes and PO Numbers from required fields

            }

            PoCreditRecords = DataModel.GetPOCreditPeriodRecords(); // get PO Credit Period Records

            VendorRecords = DataModel.GetVendorRecords();


            if (PoCreditRecords != null && RawData != null)
            {
                JoinedRawAndPOData = DataModel.JoinTablesRawAndPOCredit(RawData,PoCreditRecords.Tables[0]); //Join Processed Data with PO Credit Period Records
            }

            if (JoinedRawAndPOData != null)
            {
                FixedCPData = DataModel.MSMECreditPeriodFix(JoinedRawAndPOData);
            }

            if (FixedCPData != null)
            {
                DateTime marchQuarter = new DateTime(2025,3,20);
                DateTime last_day_current_quarter = DataModel.GetLastDateOfCurrentQuarter(marchQuarter);

                currentQuarter = last_day_current_quarter;
                DataWithAgeing = DataModel.AgeingCalculation(FixedCPData, last_day_current_quarter);
            }

            if(DataWithAgeing != null)
            {
                DatasWithHyp = DataModel.HyperionCodeClassification(DataWithAgeing);
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
                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    DocumentHeaderText = row["Document_Header"]?.ToString(),
                    Assignment = row["Assignment"]?.ToString(),
                    InvoiceReference = row["Invoice_Reference"].ToString(),
                    Source = row["SOURCE"].ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    VendorDescription = row["Vendor_Description"].ToString(),
                    InvoiceDescription = row["Invoice_DEscription"]?.ToString(),
                    DocumentType = row["Document_Type"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment_Terms"]?.ToString(),
                    AmountLocal = row["Amount_Local"]?.ToString(),
                    DocumentDate = row["Document_Date"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting_Date"].ToString(),
                    CompanyCode = row["Company_Code"].ToString(),
                    DocumentNumber = row["Document_Number"].ToString(),
                    GLAccount = row["GL_Account"].ToString(),
                    GLDescription = row["GL_Description"].ToString(),
                    ProfitCenter = row["Profit_Center"].ToString(),
                    Edited = row["Edited"].ToString()

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

                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    InvoiceDescription = row["Invoice_DEscription"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment_Terms"]?.ToString(),
                    DocumentHeaderText = row["Document_Header"]?.ToString(),
                    AmountLocal = row["Amount_Local"]?.ToString(),
                    Assignment = row["Assignment"]?.ToString(),
                    DocumentDate = row["Document_Date"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting_Date"].ToString(),
                    Processed = row["Processed"].ToString(),
                    InvoiceKey = row["Invoice_Key"].ToString()
                });
            }

            var viewModel = new ProcessedInvoicesListModel { Invoices = processedInvoices };
            return View(viewModel);
        }

        //GET Data/EditData
        public ActionResult EditData()
        {

            List<EditInvoicesViewModel> invoices = new List<EditInvoicesViewModel>();
            int idCounter = 1; // Assign a unique ID for each row for client-side tracking
            foreach (DataRow row in processedData.Rows)
            {
                invoices.Add(new EditInvoicesViewModel
                {
                    Id = idCounter++,
                    InvoiceKey = row["Invoice_Key"].ToString(),
                    DocumentType=row["Document_Type"].ToString(),
                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    DocumentHeaderText = row["Document_Header"]?.ToString(),
                    Assignment = row["Assignment"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    InvoiceDescription = row["Invoice_Description"]?.ToString(),
                    AmountLocal = row["Amount_Local"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment_Terms"]?.ToString(),
                    IsEdited = row["Edited"].ToString() 
                });
            }

            var viewModel = new EditInvoicesListModel { Invoices = invoices };
            return View(viewModel);
        }

        // This POST action will receive the edited data
        [HttpPost]
        public ActionResult SaveEditedData(List<EditInvoicesViewModel> editedInvoices)
        {
            if (editedInvoices == null)
            {
                return Json(new { success = false, message = "No data received." });
            }

            //---IMPORTANT: Process only truly edited rows-- -
            var changedInvoices = editedInvoices.Where(o => o.IsEdited == "True").ToList();


            bool success =DataModel.UpdateDatabase(changedInvoices);

            if (success)
            {
                return Json(new { success = true, message = $"{changedInvoices.Count} rows processed successfully!" });
            }
            else
            {
                return Json(new { success = false, message = $"Error processing changes!" });
            }

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
                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    PaymentTerms = row["Payment_Terms"]?.ToString(),
                    AmountLocal = row["Amount_Local"]?.ToString(),
                    DocumentDate = row["Document_Date"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting_Date"].ToString(),
                    CreditPeriod = row["Credit_Period"].ToString(),
                    InvoiceKey = row["Invoice_Key"].ToString()
                });
            }

            var viewModel = new JoinedRawAndPOInvoicesListModel { Invoices = invoices };
            return View(viewModel);
        }



        public ActionResult FixedCPForMSME()
        {
            List<EditInvoicesViewModel> invoices = new List<EditInvoicesViewModel>();
            foreach (DataRow row in FixedCPData.Rows)
            {
                invoices.Add(new EditInvoicesViewModel
                {
                    InvoiceKey = row["Invoice_Key"].ToString(),
                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    AmountLocal = row["Amount_Local"].ToString(),
                    PaymentTerms = row["Payment_Terms"].ToString(),
                    CreditPeriod = row["Credit_Period"].ToString(),
                    Processed = row["Processed"].ToString(),
                    IsEdited = row["Edited"].ToString(),
                    CPFixed = row["CP_FIxed"].ToString()
                });
            }

            var viewModel = new EditInvoicesListModel { Invoices = invoices };
            return View(viewModel);

        }

        
        public ActionResult AgedData()
        {
            List<AgedInvoicesViewModel> invoices = new List<AgedInvoicesViewModel>();
            foreach (DataRow row in DataWithAgeing.Rows)
            {
                invoices.Add(new AgedInvoicesViewModel
                {
                    InvoiceKey = row["Invoice_Key"].ToString(),
                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    AmountLocal = row["Amount_Local"].ToString(),
                    CreditPeriod = row["Credit_Period"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting_Date"].ToString(),
                    Ageing = row["Ageing"].ToString(),
                    AgeingYears = row["Ageing_Years"].ToString(),
                    AgeingGroup = row["Ageing_Group"].ToString()
                });
            }

            var viewModel = new AgedInvoicesListModel { Invoices = invoices, CurrentQuarter=currentQuarter };
            return View(viewModel);
        }

        public ActionResult InvoicesWithHypCode()
        {
            List<InvoicesWithHypCodeView> invoices = new List<InvoicesWithHypCodeView>();
            foreach (DataRow row in DatasWithHyp.Rows)
            {
                invoices.Add(new InvoicesWithHypCodeView
                {
                    InvoiceKey = row["Invoice_Key"].ToString(),
                    PurchasingDocument = row["Purchasing_Document"]?.ToString(),
                    Vendor = row["Vendor"]?.ToString(),
                    Industry = row["Industry"]?.ToString(),
                    AmountLocal = row["Amount_Local"].ToString(),
                    CreditPeriod = row["Credit_Period"].ToString(),
                    PaymentDate = row["Payment_Date"].ToString(),
                    PostingDate = row["Posting_Date"].ToString(),
                    Ageing = row["Ageing"].ToString(),
                    AgeingYears = row["Ageing_Years"].ToString(),
                    AgeingGroup = row["Ageing_Group"].ToString(),
                    GLAccount = row["GL_Account"].ToString(),
                    GLDescription = row["GL_Description"].ToString(),
                    CompanyCode = row["Company_Code"].ToString(),
                    DocumentType = row["Document_Type"].ToString(),
                    
                    Hyperion_Code = row["Hyperion_Code"].ToString(),
                    Hyp_Code_Description = row["Hyp_Code_Description"].ToString(),
                    Due_Status = row["Due_Status"].ToString(),
                    Billed_Status = row["Billed_Status"].ToString()
                });
            }

            var viewModel = new InvoicesWithHypCodeListView { Invoices = invoices, CurrentQuarter = currentQuarter };
            return View(viewModel);
        }
    }
}
