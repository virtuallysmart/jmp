using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportData
    {
        public List<string> Warnings { get; set; }

        public ReportColumnHeader[] ColumnHeaders { get; set; }

        public ReportDataRow[] Rows { get; set; }

        public DateTime EndDate { get; set; }

        public string CriticalPath { get; set; }

        public ReportData()
        {
            Warnings = new List<string>();
        }
    }
}
