using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncMon
{
    class SqlNotify_DocumentInfo
    {
        public int DocumentType { get; set; }
        public int OriginalDocumentID { get; set; }
        public int DocumentID { get; set; }
        public int PaymentMethod { get; set; }
    }
}
