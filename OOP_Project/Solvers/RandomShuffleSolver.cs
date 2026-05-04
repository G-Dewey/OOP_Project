using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    internal class RandomShuffleSolver : Solver
    {
        public RandomShuffleSolver(JobShop jobShop) : base(jobShop)
        {
            SolverName = "Random Shuffle";
        }

        public override ErrorOr<Schedule> Solve()
        {
            int[] baseGene = JobShopObj.BaseGene.ToArray();

            Random rand = new Random();
            int[] shuffledGene = baseGene.OrderBy(x => rand.Next()).ToArray();

            return Schedule.Create(JobShopObj, shuffledGene);
        }
    }
}