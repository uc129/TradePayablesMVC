using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TradePayablesMVC2017.Models
{
  
        public class MSMEReportView 
        {
            public string Hyperion_Code { get; set; }
            public string Hyp_Code_Description { get; set; }
            public string Total_Amount { get; set; }

        }

        public class MSMEReportListView
        {
            public List<MSMEReportView> Report { get; set; }
            public DateTime CurrentQuarter { get; set; }

        }
}