using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportDataRow
    {
        public string Label { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ReportDataCell[] Cells { get; set; }
    }
}
