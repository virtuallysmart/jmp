using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    public class IssueSearchResults
    {
        public int StartAt { get; set; }

        public int MaxResults { get; set; }

        public int Total { get; set; }

        public Issue[] Issues { get; set; }
    }
}
