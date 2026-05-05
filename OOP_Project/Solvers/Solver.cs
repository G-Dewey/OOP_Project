using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Presentation;
using ErrorOr;  

namespace OOP_Project
{
    abstract class Solver
    {
        protected string SolverName;
        protected JobShop JobShopObj;
        protected int[] BestGene;
        protected int BestMakespan = -1;
        
        // RANDOM
        private static readonly ThreadLocal<Random> _threadRand =
        new ThreadLocal<Random>(() => new Random());
        protected static Random rand => _threadRand.Value;

        public Solver(JobShop jobShop) 
        { 
            JobShopObj = jobShop;
        }

        public string GetSolverName()
        {
            return SolverName;
        }

        // This is the method that will be 
        public virtual ErrorOr<Schedule> Solve()
        {
            return Error.Failure(description: "Function not overwritten");
        }

        // Used to shuffle the gene for the starting point of algorithms
        protected int[] ShuffleGene(int[] gene)
        {
            return gene.OrderBy(x => rand.Next()).ToArray();
        }
    }
}
