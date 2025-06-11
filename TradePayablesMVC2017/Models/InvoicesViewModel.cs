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
        public string DocumentDate { get; set; }
        public string PaymentDate { get; set; }
        public string PostingDate { get; set; }
        public string AmountLocal { get; set; }
        public string Vendor { get; set; }
        public string Text { get; set; }
        public string Industry { get; set; }
        public string PaymentTerms { get; set; }

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
    public class EditInvoicesViewModel
    {
        public int id { get; set; } // A unique ID is crucial for identifying rows for editing
        public string PurchasingDocument { get; set; }
        public string Vendor { get; set; }       
        public string Text { get; set; }
        public string Industry { get; set; }
        public string PaymentTerms { get; set; }
        public string DocumentHeaderText { get; set; }
        public string Assignment { get; set; }
        
        // Property to indicate if the row was edited (useful for saving)
        public bool IsEdited { get; set; }
    }

    public class EditInvoicesListModel
    {
        public List<EditInvoicesViewModel> Invoices { get; set; }
    }


    /// <summary>
    /// Display Invoices joined for CreditPeriod using PO
    /// </summary>
    public class JoinedRawAndPOInvoicesViewModel : RawInvoicesViewModel 
    {
        public string CreditPeriod { get; set; }
    }

    public class JoinedRawAndPOInvoicesListModel
    {
        public List<JoinedRawAndPOInvoicesViewModel> Invoices { get; set; }
    }



}