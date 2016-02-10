using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    [Serializable]
    public class Status
    {
        public string Name { get; set; }

        public StatusCategory StatusCategory { get; set; }
    }
}
