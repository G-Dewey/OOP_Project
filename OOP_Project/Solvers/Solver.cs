using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;  

namespace OOP_Project
{
    abstract class Solver
    {
        protected string SolverName;
        protected JobShop JobShopObj;

        public Solver(JobShop jobShop) 
        { 
            JobShopObj = jobShop;
        }

        public string GetSolverName()
        {
            return SolverName;
        }   
    }
}
