using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    public static class Dictionaries
    {
        // Used to track the headers and the index in job array
        public static Dictionary<string, int> HeaderDict = new Dictionary<string, int>
        {
            {"jobid", 0},
            {"operationid", 1},
            {"subdivision", 2},
            {"processingtime", 3}
        };

        // Used to track the headers and the index in job array
        public static Dictionary<string, int> Ma = new Dictionary<string, int> { };
    }
}
