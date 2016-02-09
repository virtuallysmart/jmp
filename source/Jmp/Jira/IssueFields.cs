using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    public class IssueFields
    {
        public string Summary { get; set; }

        public Priority Priority { get; set; }

        public string[] Labels { get; set; }

        public TimeTracking TimeTracking { get; set; }
    }
}
