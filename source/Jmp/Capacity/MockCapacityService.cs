using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Capacity
{
    public class MockCapacityService : ICapacityService
    {
        public IDictionary<string, double> GetWeeklyCapacityByStream()
        {
            var capacity = new Dictionary<string, double>()
            {
                { "Bart", 30 }
            };
            return capacity;
        }
    }
}
