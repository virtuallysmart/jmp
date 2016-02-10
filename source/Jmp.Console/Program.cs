using Jmp.Jira;
using Jmp.Reports;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var setup = new ReportSetup()
            {
                JiraApiUrl = "https://orwell.atlassian.net/rest/api/2/",
                JiraUserName = "YOUR USER HERE",
                JiraPassword = "YOUR PASSWORD HERE",
                Jql = "labels=jmp",
                ColumnLabelPrefix = "jmp-stream-"
            };

            var jiraClient = new JiraClient();
            var issues = jiraClient.GetIssues(setup.JiraApiUrl, setup.JiraUserName, setup.JiraPassword, setup.Jql);

            var reportService = new ReportService();
            var capacity = new Dictionary<string, int>() { { "*", 40 } };
            var issueFinalStatuses = new string[] { "DEV COMPLETE", "CANCELLED" };
            var report = reportService.GetReportData(issues, setup.ColumnLabelPrefix, capacity, issueFinalStatuses);
        }

        public class ReportSetup
        {
            public string JiraApiUrl { get; set; }

            public string JiraUserName { get; set; }

            public string JiraPassword { get; set; }

            public string Jql { get; set; }

            public string ColumnLabelPrefix { get; set; }

            public string ColumnUnassignedLabel { get; set; }
        }
    }
}
