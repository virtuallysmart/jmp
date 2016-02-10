using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    [Serializable]
    public class TimeTracking
    {
        public string OriginalEstimate { get; set; }

        public string RemainingEstimate { get; set; }

        public string TimeSpent { get; set; }

        public long OriginalEstimateSeconds { get; set; }

        public long RemainingEstimateSeconds { get; set; }

        public long TimeSpentSeconds { get; set; }
    }
}
