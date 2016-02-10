using Jmp.Capacity;
using Jmp.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportService : IReportService
    {
        private readonly ICapacityService _capacityService;
        private readonly string[] _endStatuses;

        public ReportService(ICapacityService capacityService)
        {
            _capacityService = capacityService;
            _endStatuses = new string[] { "DEV COMPLETE", "CANCELLED" };
        }

        public ReportData GetReportData(Issue[] issues, string columnLabelPrefix)
        {
            var reportData = new ReportData();

            var checkClosedWithEstimateCount = issues.Count(i => CheckClosedWithEstimate(i));
            if (checkClosedWithEstimateCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} closed issues with remaining estimate in the data set. They have not been included in the report.", checkClosedWithEstimateCount));
            }

            var openIssues = issues.Where(i => !_endStatuses.Any(s => s.Equals(i.Fields.Status.Name, StringComparison.InvariantCultureIgnoreCase))).ToArray();

            var checkHasNoEstimateCount = openIssues.Count(i => CheckHasNoEstimate(i));
            var checkIsNotAssignedCount = openIssues.Count(i => CheckIsNotAssigned(i, columnLabelPrefix));
            var checkOverburnedCount = openIssues.Count(i => CheckOverburned(i));
            if (checkHasNoEstimateCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} unestimated issues in the data set. They have not been included in the report.", checkHasNoEstimateCount));
            }
            if (checkIsNotAssignedCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} unassigned issues in the data set. They have not been included in the report.", checkIsNotAssignedCount));
            }
            if (checkOverburnedCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} overburned issues in the data set. They have not been included in the report.", checkOverburnedCount));
            }

            var validIssues = openIssues.Where(i =>
                !CheckHasNoEstimate(i) &&
                !CheckIsNotAssigned(i, columnLabelPrefix) &&
                !CheckOverburned(i))
                .ToArray();

            if (openIssues.Length > validIssues.Length)
            {
                reportData.Warnings.Add(string.Format("There are {0} open issues in the data set and only {1} are included in the report.", openIssues.Length, validIssues.Length));
            }

            var issueGroups = validIssues.GroupBy(i =>
                {
                    var columnLabel = i.Fields.Labels.FirstOrDefault(l => l.StartsWith(columnLabelPrefix));
                    if (string.IsNullOrEmpty(columnLabel))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        columnLabel = columnLabel.Substring(columnLabelPrefix.Length);
                    }
                    return columnLabel;
                })
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Fields.Priority.Name).ToArray());

            var weeklyCapacitySeconds = new Dictionary<string, long>();
            foreach (var g in issueGroups)
            {
                weeklyCapacitySeconds.Add(g.Key, _capacityService.GetWeeklyCapacitySeconds(g.Key));
            }

            var columnHeaders =
                (from g in issueGroups
                 select new ReportColumnHeader()
                  {
                      Label = g.Key,
                      TotalIssueCount = g.Value.Length,
                      TotalRemainingSeconds = g.Value.Sum(v => v.Fields.TimeTracking.RemainingEstimateSeconds),
                      TotalCapacity = weeklyCapacitySeconds[g.Key]
                  })
                  .ToArray();

            foreach (var c in columnHeaders)
            {
                c.TotalWorkWeeks = (int)Math.Ceiling(c.TotalRemainingSeconds / c.TotalCapacity);
            }

            var maxWeeks = columnHeaders.Max(c => c.TotalWorkWeeks);

            var skips = new Dictionary<string, int>();
            foreach (var g in issueGroups)
            {
                skips.Add(g.Key, 0);
            }

            var rows = new List<ReportDataRow>();
            for (int i = 0; i < maxWeeks; i++)
            {
                var row = new ReportDataRow()
                {
                    Label = string.Format("Week {0}", i + 1)
                };
                var rowCells = new List<ReportDataCell>();
                foreach (var g in issueGroups)
                {
                    var rowCell = new ReportDataCell();
                    rowCell.CapacitySeconds = weeklyCapacitySeconds[g.Key];
                    rowCell.Label = g.Key;
                    var rowCellIssues = new List<Issue>();
                    foreach (var issue in g.Value.Skip(skips[g.Key]))
                    {
                        rowCellIssues.Add(issue);
                        skips[g.Key]++;
                        var totalRemainingSeconds = rowCellIssues.Sum(item => item.Fields.TimeTracking.RemainingEstimateSeconds);
                        if (totalRemainingSeconds == weeklyCapacitySeconds[g.Key])
                        {
                            break;
                        }
                        else if (totalRemainingSeconds > weeklyCapacitySeconds[g.Key])
                        {
                            skips[g.Key]--;
                            break;
                        }
                    }
                    rowCell.Issues = rowCellIssues.ToArray();
                    rowCell.RemainingEstimateSeconds = rowCellIssues.Sum(item => item.Fields.TimeTracking.RemainingEstimateSeconds);
                    rowCells.Add(rowCell);
                }
                row.Cells = rowCells.ToArray();
                rows.Add(row);
            }

            reportData.ColumnHeaders = columnHeaders;
            reportData.Rows = rows.ToArray();
            reportData.EndDate = DateTime.Today.AddDays(7 * rows.Count);
            reportData.CriticalPath = string.Join(", ", rows.Last().Cells.Where(c => c.Issues.Length > 0).Select(c => c.Label).ToArray());
            return reportData;
        }

        #region issue checks

        private bool CheckClosedWithEstimate(Issue i)
        {
            return _endStatuses.Any(s => s.Equals(i.Fields.Status.Name, StringComparison.InvariantCultureIgnoreCase)) && i.Fields.TimeTracking.RemainingEstimateSeconds > 0;
        }

        private bool CheckHasNoEstimate(Issue i)
        {
            return string.IsNullOrEmpty(i.Fields.TimeTracking.RemainingEstimate);
        }

        private bool CheckIsNotAssigned(Issue i, string columnLabelPrefix)
        {
            return !i.Fields.Labels.Any(l => l.StartsWith(columnLabelPrefix));
        }

        private bool CheckOverburned(Issue i)
        {
            return i.Fields.TimeTracking.RemainingEstimateSeconds == 0;
        }

        #endregion
    }
}