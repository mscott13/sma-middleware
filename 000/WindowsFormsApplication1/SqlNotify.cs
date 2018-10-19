using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncMon
{
    class SqlNotify
    {
        public int ARInvoiceID { get; set; } //Document Number
        public int CreditGLID { get; set; }
        public int DebitGLID { get; set; }
        public string CustomerID { get; set; } //Id used to get name
        public string Amount { get; set; } //Amount
        public string ARBalance { get; set; }
        public string FeeType { get; set; } //Spectrum fee or Regulatory Fee
        public string notes { get; set; }
    }
}
