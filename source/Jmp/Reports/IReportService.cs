using Jmp.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Reports
{
    public interface IReportService
    {
        ReportData GetReportData(Issue[] issues, string columnNamePrefix, string columnUnassignedLabel, IDictionary<string, double> weeklyCapacityByColumn);
    }
}
