using Jmp.Jira;
using Jmp.Reports;
using Jmp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jmp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IJiraClient _jiraClient;
        private readonly IReportService _reportService;
        private readonly ReportSetup _defaultReportSetup;

        public HomeController(IJiraClient jiraClient, IReportService reportService, ReportSetup defaultReportSetup)
        {
            _jiraClient = jiraClient;
            _reportService = reportService;
            _defaultReportSetup = defaultReportSetup;
        }

        public ActionResult Index()
        {
            return View(_defaultReportSetup);
        }

        public ActionResult Report()
        {
            var setup = Session["setup"] as ReportSetup;
            if (setup == null)
            {
                return RedirectToAction("Index");
            }
            var issues = _jiraClient.GetIssues(setup.JiraApiUrl, setup.JiraUserName, setup.JiraPassword, setup.Jql);
            var capacity = ParseCapacitySetup(setup.WeeklyCapacityHoursPerStream);
            var issueFinalStatuses = ParseIssueFinalStatuses(setup.IssueFinalStatuses);
            var reportData = _reportService.GetReportData(issues, setup.ColumnLabelPrefix, capacity, issueFinalStatuses);
            var model = new ReportModel()
            {
                ReportSetup = setup,
                ReportData = reportData
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult JiraLogin(ReportSetup setup)
        {
            if (setup == null)
            {
                return new HttpUnauthorizedResult();
            }
            Session["setup"] = setup;
            return RedirectToAction("Report");
        }

        //e.g. *: 30
        //e.g. Bart: 30, Kasia: 30, Tom: 20
        private static IDictionary<string, int> ParseCapacitySetup(string input)
        {
            var capacity = new Dictionary<string, int>();
            var byStream = input.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (byStream.Length == 0)
            {
                throw new ArgumentException("Invalid capacity setup format");
            }
            foreach (var bs in byStream)
            {
                var s = bs.Trim().Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (s.Length == 0)
                {
                    throw new ArgumentException("Invalid capacity setup format");
                }
                capacity.Add(s[0], int.Parse(s[1]));
            }
            return capacity;
        }

        //e.g. DEV COMPLETE, CANCELLED
        private static string[] ParseIssueFinalStatuses(string input)
        {
            return input.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
    }
}