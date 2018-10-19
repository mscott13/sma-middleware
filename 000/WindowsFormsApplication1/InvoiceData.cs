using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncMon
{
    class InvoiceData
    {
        public string CustomerID { get; set; }
        public string ARInvoiceID { get; set; }
        public decimal Amount { get; set; }
        public string FeeType { get; set; }
        public string notes { get; set; }
        public int isvoid { get; set; }
        public int isCreditMemo { get; set;}
    }
}
