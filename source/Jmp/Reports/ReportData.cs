using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportData
    {
        public ReportColumnHeader[] ColumnHeaders { get; set; }

        public ReportDataRow[] Rows { get; set; }
    }
}
