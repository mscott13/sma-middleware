using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncMon
{
    static class TableSections
    {
        private static List<DataWrapper> tsections = null;
        public static  void insertTable(List<DataWrapper> data)
        {
            tsections = data;
        }

        public static List<DataWrapper> getTable()
        {
            return tsections;
        }
    }
}
