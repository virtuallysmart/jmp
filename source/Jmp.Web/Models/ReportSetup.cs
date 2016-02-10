using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jmp.Web.Models
{
    public class ReportSetup
    {
        public string JiraUserName { get; set; }

        public string JiraPassword { get; set; }

        public string Jql { get; set; }

        public string ColumnLabelPrefix { get; set; }

        public string WeeklyCapacityHoursPerStream { get; set; }

        public string IssueFinalStatuses { get; set; }

        public string JiraApiUrl { get; set; }

        public string JiraShowIssueUrl { get; set; }

        public string JiraSearchIssuesUrl { get; set; }
    }
}