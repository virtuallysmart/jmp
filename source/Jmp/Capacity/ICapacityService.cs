﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Capacity
{
    public interface ICapacityService
    {
        long GetWeeklyCapacitySeconds(string stream);
    }
}
