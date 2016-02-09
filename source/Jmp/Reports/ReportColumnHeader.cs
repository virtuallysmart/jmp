using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportColumnHeader
    {
        public string Label { get; set; }

        public long TotalIssueCount { get; set; }
        
        public long TotalRemainingSeconds { get; set; }

        public double TotalCapacity { get; set; }

        public int TotalWorkWeeks { get; set; }
    }
}
