﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication4
{
    public class DeferredData
    {
        public List<DataWrapper> Categories { get; set; }
        public List<String> ColumnNames { get; set; }
        public Totals Total { get; set; }
        public string report_id { get; set; }

        public DeferredData()
        {

        }
    }
}