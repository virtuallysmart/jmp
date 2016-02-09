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
        public ReportData GetReportData(Issue[] issues, string columnNamePrefix, string columnUnassignedLabel, IDictionary<string, double> weeklyCapacityByColumn)
        {
            var issueGroups = issues.GroupBy(i =>
                {
                    var columnLabel = i.Fields.Labels.FirstOrDefault(l => l.StartsWith(columnNamePrefix));
                    if (string.IsNullOrEmpty(columnLabel))
                    {
                        columnLabel = columnUnassignedLabel;
                    }
                    else
                    {
                        columnLabel = columnLabel.Substring(columnNamePrefix.Length);
                    }
                    return columnLabel;
                })
                .ToDictionary(ig => ig.Key, ig => ig.OrderBy(i => i.Fields.Priority.Name).ToArray());

            foreach (var g in issueGroups)
            {
                if (!g.Key.Equals(columnUnassignedLabel, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!weeklyCapacityByColumn.ContainsKey(g.Key))
                    {
                        throw new ApplicationException(string.Format("Capacity not specified for {0}", g.Key));
                    }
                }
            }

            var columnHeaders =
                (from g in issueGroups
                 select new ReportColumnHeader()
                  {
                      Label = g.Key,
                      TotalIssueCount = g.Value.Length,
                      TotalRemainingSeconds = g.Value.Sum(v => v.Fields.TimeTracking.RemainingEstimateSeconds),
                      TotalCapacity = weeklyCapacityByColumn[g.Key]
                  })
                  .ToArray();

            foreach (var c in columnHeaders)
            {
                c.TotalWorkWeeks = (int)Math.Ceiling(c.TotalRemainingSeconds / 60 / 60 / c.TotalCapacity);
            }

            var maxWeeks = columnHeaders.Max(c => c.TotalWorkWeeks);

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
                    var rowCellIssues = new List<Issue>();
                    foreach (var issue in g.Value)
                    {
                        rowCellIssues.Add(issue);
                        var totalRemainingSeconds = rowCellIssues.Sum(item => item.Fields.TimeTracking.RemainingEstimateSeconds);
                        if (totalRemainingSeconds / 60 / 60 >= weeklyCapacityByColumn[g.Key])
                        {
                            break;
                        }
                    }
                    rowCell.Issues = rowCellIssues.ToArray();
                    rowCells.Add(rowCell);
                }
                row.Cells = rowCells.ToArray();
                rows.Add(row);
            }

            return new ReportData()
            {
                ColumnHeaders = columnHeaders,
                Rows = rows.ToArray()
            };
        }
    }
}
