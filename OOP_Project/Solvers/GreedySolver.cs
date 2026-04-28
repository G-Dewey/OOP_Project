using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    internal class GreedySolver : Solver
    {
        public GreedySolver(JobShop jobShop) : base(jobShop)
        {
            SolverName = "Greedy Hill Climber";
        }
    }
}
