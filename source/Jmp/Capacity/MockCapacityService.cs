using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Capacity
{
    public class MockCapacityService : ICapacityService
    {
        public long GetWeeklyCapacitySeconds(string stream)
        {
            return 40 * 60 * 60;
        }
    }
}
