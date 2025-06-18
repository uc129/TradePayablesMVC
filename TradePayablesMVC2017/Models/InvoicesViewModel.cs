using System.Collections.Generic;
using System;

namespace TradePayablesMVC2017.Models
{

    /// <summary>
    ///First View raw data
    /// </summary>
    public class RawInvoicesViewModel
    {
        public string PurchasingDocument { get; set; }
        public string DocumentHeaderText { get; set; }
        public string Assignment { get; set; }
        public string InvoiceReference { get; set; }
        public string DocumentType { get; set; }
        public string DocumentDate { get; set; }
        public string PaymentDate { get; set; }
        public string PostingDate { get; set; }
        public string AmountLocal { get; set; }
        public string Vendor { get; set; }
        public string VendorDescription { get; set; }
        public string InvoiceDescription { get; set; }
        public string Industry { get; set; }
        public string PaymentTerms { get; set; }
        public string GLAccount { get; set; }
        public string GLDescription { get; set; }
        public string CompanyCode { get; set; }
        public string DocumentNumber { get; set; }
        public string ProfitCenter { get; set; }
        public string Source { get; set; }
        public string Edited { get; set; }
        public string InvoiceKey { get; set; }
    }
    public class RawInvoicesListModel
    {
        public List <RawInvoicesViewModel> Invoices { get; set; }
    }


    /// <summary>
    /// 2nd View Processed Data
    /// </summary>
    /// 
    public class ProcessedInvoicesViewModel :RawInvoicesViewModel
    {
      public string Processed { get; set; }  
    }
    public class ProcessedInvoicesListModel
    {
        public List<ProcessedInvoicesViewModel> Invoices { get; set; }
    }


    /// <summary>
    /// 3rd Allow for editing of processed data
    /// </summary>
    public class EditInvoicesViewModel :ProcessedInvoicesViewModel
    {
        public int Id { get; set; } // A unique ID is crucial for identifying rows for editing
        public string CreditPeriod { get; set; }
        public string IsEdited { get; set; }
        public string CPFixed { get; set; }


    }
    public class EditInvoicesListModel
    {
        public List<EditInvoicesViewModel> Invoices { get; set; }
    }


    /// <summary>
    /// Display Invoices joined for CreditPeriod using PO
    /// </summary>
    public class JoinedRawAndPOInvoicesViewModel : EditInvoicesViewModel 
    {
    }
    public class JoinedRawAndPOInvoicesListModel
    {
        public List<JoinedRawAndPOInvoicesViewModel> Invoices { get; set; }
    }


    public class AgedInvoicesViewModel: EditInvoicesViewModel
    {
        public string Ageing { get; set; }
        public string AgeingYears { get; set; }
        public string AgeingGroup { get; set; }
    }

    public class AgedInvoicesListModel
    {
        public List <AgedInvoicesViewModel> Invoices { get; set; }
        public DateTime CurrentQuarter { get; set; }

    }


    public class  InvoicesWithHypCodeView:AgedInvoicesViewModel
    {
        public string Hyperion_Code { get; set; }
        public string Hyp_Code_Description { get; set; }
        public string Due_Status { get; set; }
        public string Billed_Status { get; set; }

    }

    public class InvoicesWithHypCodeListView
    {
        public List<InvoicesWithHypCodeView> Invoices { get; set; }
        public DateTime CurrentQuarter { get; set; }

    }




}