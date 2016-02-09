using Jmp.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportDataCell
    {
        public Issue[] Issues { get; set; }

        public long RemainingEstimateSeconds { get; set; }
    }
}
