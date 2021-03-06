﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    [Serializable]
    public class Issue
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Self { get; set; }

        public IssueFields Fields { get; set; }

        public string Origin { get; set; }
    }
}
