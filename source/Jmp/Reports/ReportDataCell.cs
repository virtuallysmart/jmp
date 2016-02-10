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
        public string Label { get; set; }

        public Issue[] Issues { get; set; }

        public long UsedSeconds { get; set; }

        public long CapacitySeconds { get; set; }
    }
}
