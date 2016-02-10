using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jmp.Web.Infrastructure
{
    public static class StringHelper
    {
        public static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}