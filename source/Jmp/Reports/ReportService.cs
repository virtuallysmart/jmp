using Jmp.Jira;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public class ReportService : IReportService
    {
        private readonly string[] _issueFinalStatuses = new string[] { "DEV COMPLETE", "CANCELLED" };

        public ReportData GetReportData(Issue[] issues, string columnLabelPrefix, IDictionary<string, int> weeklyCapacityHoursPerStream)
        {
            var reportData = new ReportData();

            var checkClosedWithEstimateCount = issues.Count(i => CheckClosedWithEstimate(i));
            if (checkClosedWithEstimateCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} closed issues with remaining estimate in the dataset. They have not been included in the report.", checkClosedWithEstimateCount));
            }

            var openIssues = issues.Where(i => !_issueFinalStatuses.Any(s => s.Equals(i.Fields.Status.Name, StringComparison.InvariantCultureIgnoreCase))).ToArray();

            var checkHasNoEstimateCount = openIssues.Count(i => CheckHasNoEstimate(i));
            var checkIsNotAssignedCount = openIssues.Count(i => CheckIsNotAssigned(i, columnLabelPrefix));
            var checkOverburnedCount = openIssues.Count(i => CheckOverburned(i));
            if (checkHasNoEstimateCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} unestimated issues in the dataset. They have not been included in the report.", checkHasNoEstimateCount));
            }
            if (checkIsNotAssignedCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} unassigned issues in the dataset. They have not been included in the report.", checkIsNotAssignedCount));
            }
            if (checkOverburnedCount > 0)
            {
                reportData.Warnings.Add(string.Format("There are {0} overburned issues in the dataset. They have not been included in the report.", checkOverburnedCount));
            }

            var validIssues = openIssues.Where(i =>
                !CheckHasNoEstimate(i) &&
                !CheckIsNotAssigned(i, columnLabelPrefix) &&
                !CheckOverburned(i))
                .ToArray();

            if (openIssues.Length > validIssues.Length)
            {
                reportData.Warnings.Add(string.Format("There are {0} open issues in the dataset and only {1} are included in the report.", openIssues.Length, validIssues.Length));
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

            var weeklyCapacitySecondsPerStream = new Dictionary<string, long>();
            foreach (var g in issueGroups)
            {
                var capacity = 0;
                if (weeklyCapacityHoursPerStream.ContainsKey("*"))
                {
                    capacity = weeklyCapacityHoursPerStream["*"];
                }
                else
                {
                    capacity = weeklyCapacityHoursPerStream[g.Key];
                }
                weeklyCapacitySecondsPerStream.Add(g.Key, capacity * 60 * 60);
            }

            var columnHeaders =
                (from g in issueGroups
                 select new ReportColumnHeader()
                  {
                      Label = g.Key,
                      TotalIssueCount = g.Value.Length,
                      TotalRemainingSeconds = g.Value.Sum(v => v.Fields.TimeTracking.RemainingEstimateSeconds),
                      TotalCapacity = weeklyCapacitySecondsPerStream[g.Key]
                  })
                  .ToArray();

            foreach (var c in columnHeaders)
            {
                c.TotalWorkWeeks = (int)Math.Ceiling(c.TotalRemainingSeconds / c.TotalCapacity);
            }

            var maxWeeks = columnHeaders.Max(c => c.TotalWorkWeeks);

            var handledIssues = new Dictionary<string, int>();
            foreach (var g in issueGroups)
            {
                handledIssues.Add(g.Key, 0);
            }
            var partialIssueRemainingEstimates = new Dictionary<string, long>();
            foreach (var g in issueGroups)
            {
                partialIssueRemainingEstimates.Add(g.Key, 0);
            }

            var rows = new List<ReportDataRow>();
            var date = DateTime.Today;
            for (int i = 0; i < maxWeeks; i++)
            {
                var row = new ReportDataRow()
                {
                    Label = string.Format("Week {0}", i + 1),
                    StartDate = date,
                    EndDate = date.AddDays(7)
                };
                date = date.AddDays(7);
                var rowCells = new List<ReportDataCell>();
                foreach (var g in issueGroups)
                {
                    var rowCell = new ReportDataCell();
                    rowCell.CapacitySeconds = weeklyCapacitySecondsPerStream[g.Key];
                    rowCell.Label = g.Key;
                    var rowCellIssues = new List<Issue>();
                    foreach (var issue in g.Value.Skip(handledIssues[g.Key]))
                    {
                        var usedSeconds = rowCellIssues.Sum(ci => ci.Fields.TimeTracking.RemainingEstimateSeconds);
                        var currentIssue = issue;
                        if (partialIssueRemainingEstimates[g.Key] > 0)
                        {
                            currentIssue = GetPartialIssue(issue, partialIssueRemainingEstimates[g.Key]);
                            var availableSeconds = weeklyCapacitySecondsPerStream[g.Key] - usedSeconds;
                            if (availableSeconds > partialIssueRemainingEstimates[g.Key])
                            {
                                partialIssueRemainingEstimates[g.Key] = 0;
                            }
                            else
                            {
                                partialIssueRemainingEstimates[g.Key] = partialIssueRemainingEstimates[g.Key] - availableSeconds;
                            }
                        }
                        var issueSeconds = currentIssue.Fields.TimeTracking.RemainingEstimateSeconds;
                        if (usedSeconds + issueSeconds < weeklyCapacitySecondsPerStream[g.Key])
                        {
                            rowCellIssues.Add(currentIssue);
                            handledIssues[g.Key]++;
                        }
                        else if (usedSeconds + issueSeconds == weeklyCapacitySecondsPerStream[g.Key])
                        {
                            rowCellIssues.Add(currentIssue);
                            handledIssues[g.Key]++;
                            break;
                        }
                        else if (usedSeconds + issueSeconds > weeklyCapacitySecondsPerStream[g.Key])
                        {
                            var availableSeconds = weeklyCapacitySecondsPerStream[g.Key] - usedSeconds;
                            partialIssueRemainingEstimates[g.Key] = issueSeconds - availableSeconds;
                            var partialIssue = GetPartialIssue(issue, availableSeconds);
                            rowCellIssues.Add(partialIssue);
                            break;
                        }
                    }
                    rowCell.UsedSeconds = rowCellIssues.Sum(ci => ci.Fields.TimeTracking.RemainingEstimateSeconds);
                    rowCell.Issues = rowCellIssues.ToArray();
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

        #region Helpers

        private bool CheckClosedWithEstimate(Issue i)
        {
            return _issueFinalStatuses.Any(s => s.Equals(i.Fields.Status.Name, StringComparison.InvariantCultureIgnoreCase)) && i.Fields.TimeTracking.RemainingEstimateSeconds > 0;
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

        private static Issue GetPartialIssue(Issue original, long remainingEstimateSeconds)
        {
            Issue copy = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, original);
                ms.Position = 0;
                copy = (Issue)formatter.Deserialize(ms);
            }
            copy.Fields.Summary = string.Format("{{PARTIAL!}} {0}", copy.Fields.Summary);
            copy.Fields.TimeTracking.RemainingEstimateSeconds = remainingEstimateSeconds;
            copy.Fields.TimeTracking.RemainingEstimate = string.Format("{0}h", remainingEstimateSeconds / 60 / 60);
            return copy;
        }

        #endregion
    }
}