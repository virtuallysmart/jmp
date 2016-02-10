using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jmp.Web.Models
{
    public class ReportSetup
    {
        public string JiraUserName { get; set; }

        public string JiraPassword { get; set; }

        public string JiraApiUrl { get; set; }

        public string JiraBrowseUrl { get; set; }

        public string Jql { get; set; }

        public string ColumnLabelPrefix { get; set; }        
    }
}