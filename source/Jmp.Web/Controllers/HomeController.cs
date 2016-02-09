using Jmp.Capacity;
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
        private readonly ICapacityService _capacityService;
        private readonly ReportSetup _defaultReportSetup;

        public HomeController(IJiraClient jiraClient, IReportService reportService, ICapacityService capacityService, ReportSetup defaultReportSetup)
        {
            _jiraClient = jiraClient;
            _reportService = reportService;
            _capacityService = capacityService;
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
            var issues = _jiraClient.GetIssues(setup.JiraApiUrl, setup.JiraUserName, setup.JiraPassword, setup.IncludeLabel);
            var capacity = _capacityService.GetWeeklyCapacityByStream();
            var report = _reportService.GetReportData(issues, setup.ColumnNamePrefix, setup.ColumnUnassignedLabel, capacity);
            return View(report);
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
    }
}