using ErrorOr;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    internal class HillClimberSolver : Solver
    {
        private const int _maxStag = 1000; 
        private int[] _gene;
        private int[] _baseGene;

        public HillClimberSolver(JobShop jobShop) : base(jobShop)
        {
            SolverName = "Greedy Hill Climber";
            _baseGene = JobShopObj.BaseGene.ToArray();
        }

        // Swaps two genes by index
        private void Swap(int i, int j)
        {
            int temp = _gene[i];
            _gene[i] = _gene[j];
            _gene[j] = temp;
        }

        // Performs a random swap mutation
        private void SwapRandom()
        {
            int i = rand.Next(0, _gene.Length);
            int j = rand.Next(0, _gene.Length);
            Swap(i, j);
        }

        public override ErrorOr<Schedule> Solve()
        {
            _gene = ShuffleGene(_baseGene);

            int[] bestGene = _gene.ToArray();
            int bestFitness = JobShopObj.EvaluateGene(_gene);
            int stagnantIterations = 0;

            // Iterative local search (Hill Climbing)
            while (stagnantIterations < _maxStag)
            {
                int[] previousGene = _gene.ToArray();

                SwapRandom();

                int newFitness = JobShopObj.EvaluateGene(_gene);

                // If mutation improves the makespan, keep it
                if (newFitness < bestFitness)
                {
                    bestFitness = newFitness;
                    bestGene = _gene.ToArray();
                    stagnantIterations = 0;
                }
                else
                {
                    // Otherwise revert and increment stagnation counter
                    _gene = previousGene;
                    stagnantIterations++;
                }
            }

            return Schedule.Create(JobShopObj, bestGene);
        }
    }
}