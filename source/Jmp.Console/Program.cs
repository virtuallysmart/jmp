using Jmp.Capacity;
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
                IncludeLabel = "jmp",
                ColumnNamePrefix = "jmp-stream-",
                ColumnUnassignedLabel = "Unassigned"
            };

            var jiraClient = new JiraClient();
            var issues = jiraClient.GetIssues(setup.JiraApiUrl, setup.JiraUserName, setup.JiraPassword, setup.IncludeLabel);

            var capacityService = new MockCapacityService();
            var capacity = capacityService.GetWeeklyCapacityByStream();

            var reportService = new ReportService();
            var report = reportService.GetReportData(issues, setup.ColumnNamePrefix, setup.ColumnUnassignedLabel, capacity);
        }

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
}
