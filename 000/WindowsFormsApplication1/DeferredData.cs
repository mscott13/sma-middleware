using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncMon
{
    public class DeferredData
    {
        public List<DataWrapper> tables { get; set; }
        public Totals total { get; set; }

        public DeferredData(List<DataWrapper> _tables, Totals _totals)
        {
            tables = _tables;
            total = _totals;
        }
    }
}
