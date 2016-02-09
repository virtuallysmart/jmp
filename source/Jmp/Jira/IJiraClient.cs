using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    public interface IJiraClient
    {
        Issue[] GetIssues(string jiraApiUrl, string jiraUserName, string jiraPassword, string includeLabel);
    }
}
