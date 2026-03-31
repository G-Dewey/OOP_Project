using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    public static class Dictionaries
    {
        // Used to track the subDivision name to an int 
        public static Dictionary<string, int> subdivisionDict = new Dictionary<string, int>();

        // Used to track the headers and the index in job array
        public static Dictionary<string, int> headerDict = new Dictionary<string, int>
        {
            {"jobid", 0},
            {"operation", 1},
            {"subdivision", 2},
            {"processingtime", 3}
        };
    }
}
