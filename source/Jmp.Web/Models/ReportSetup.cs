using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jmp.Web.Models
{
    public class ReportSetup
    {
        public string JiraApiUrl { get; set; }

        public string JiraUserName { get; set; }

        public string JiraPassword { get; set; }

        public string IncludeLabel { get; set; }

        public string ColumnNamePrefix { get; set; }

        public string ColumnUnassignedLabel { get; set; }
    }
}