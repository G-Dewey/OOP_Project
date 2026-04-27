using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    static class Globals
    {
        public static DateTime StartTime = new DateTime(2024, 4, 1, 9, 0, 0);
        
        public static Dictionary<string, Func<ISolver>> Solvers = new Dictionary<string, Func<ISolver>>() 
        {
            { "Greedy Hill Climber" , () => new GreedyHillClimberSolver() }, }
            { "Genetic" , () => new GeneticSolver() }
        };
    }
}
