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
            var reportData = _reportService.GetReportData(issues, setup.ColumnLabelPrefix);
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
    }
}