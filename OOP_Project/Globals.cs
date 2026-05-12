using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    public static class Globals
    {
        public static DateTime StartTime = new DateTime(2024, 4, 1, 9, 0, 0);

        public static Dictionary<string, int> HeaderDict = new Dictionary<string, int>
        {
            {"jobid", 0},
            {"operationid", 1},
            {"subdivision", 2},
            {"processingtime", 3}
        };

        public static SolverFactory SolverFactory = new SolverFactory();

        public static string[] AvailableAlgorithms = SolverFactory.GetAvailableAlgorithms();
    }
}
