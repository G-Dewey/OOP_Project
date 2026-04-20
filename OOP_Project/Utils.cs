using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    internal static class Utils
    {
        public static DateTime AddDuration(DateTime start, int duration)
        {
            return start.AddMinutes(duration * 60);
        }
    }
}
