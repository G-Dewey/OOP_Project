using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    internal class GeneticSolver : Solver
    {
        // Hyperparameters
        private int _populationSize = 60;
        private double _mutationRate = 0.05;
        private int _maxGenerations = 300;
        private int _tournamentSize = 4;
        private int _elitismCount = 2;
        private int _maxNoImprovement = 50;

        public GeneticSolver(JobShop jobShop) : base(jobShop)
        {
            SolverName = "Genetic";
        }

        // Generates the starting pool of candidate solutions
        private List<int[]> InitialisePopulation()
        {
            List<int[]> population = new List<int[]>();
            int[] baseGene = JobShopObj.BaseGene.ToArray();

            for (int i = 0; i < _populationSize; i++)
            {
                population.Add(ShuffleGene(baseGene));
            }

            return population;
        }

        // Picks the best individual from a random subset
        private int[] TournamentSelection(List<int[]> population, JobShop tempJobShop, Dictionary<int[], int> fitness)
        {
            List<int[]> tournament = new List<int[]>();

            for (int i = 0; i < _tournamentSize; i++)
            {
                tournament.Add(population[rand.Next(population.Count)]);
            }

            tournament.Sort((a, b) => fitness[a].CompareTo(fitness[b]));
            return tournament[0];
        }

        // Using Precedence Preservative Crossover (PPX)
        private (int[], int[]) Crossover(int[] parent1, int[] parent2)
        {
            int length = parent1.Length;
            int[] child1 = new int[length];
            int[] child2 = new int[length];

            // Assign placeholders to children
            for (int i = 0; i < length; i++)
            {
                child1[i] = -1;
                child2[i] = -1;
            }

            // Removes duplicates from parents to get unique values
            var uniqueValues = parent1.Distinct().ToList();

            // Splits the unique values into two random sets
            HashSet<int> set1 = new HashSet<int>();
            HashSet<int> set2 = new HashSet<int>();

            foreach (var value in uniqueValues)
            {
                if (rand.NextDouble() < 0.5) { set1.Add(value); }
                else { set2.Add(value); }
            }

            // Checks in edge case
            if (set1.Count == 0)
            {
                set2.Remove(uniqueValues[0]);
                set1.Add(uniqueValues[0]);
            }

            if (set2.Count == 0)
            {
                set1.Remove(uniqueValues[0]);
                set2.Add(uniqueValues[0]);
            }

            for (int i = 0; i < length; i++)
            {
                if (set1.Contains(parent1[i]))
                {
                    child1[i] = parent1[i];
                }

                if (set2.Contains(parent2[i]))
                {
                    child2[i] = parent2[i];
                }
            }

            // used to keep track of the position in the parents
            int p1Index = 0;
            int p2Index = 0;

            for (int i = 0; i < length; ++i)
            {
                if (child1[i] == -1)
                {
                    // Cycle to find the next gene from parent2 that is not in set1
                    while (set1.Contains(parent2[p2Index]))
                    {
                        p2Index++;
                    }
                    child1[i] = parent2[p2Index];
                    p2Index++;
                }

                if (child2[i] == -1)
                {
                    // Cycle to find the next gene from parent1 that is not in set2
                    while (set2.Contains(parent1[p1Index]))
                    {
                        p1Index++;
                    }
                    child2[i] = parent1[p1Index];
                    p1Index++;
                }
            }

            return (child1, child2);
        }

        // Randomly swaps genes based on mutation rate
        private void Mutate(ref int[] gene)
        {
            for (int i = 0; i < gene.Length; i++)
            {
                if (rand.NextDouble() < _mutationRate)
                {
                    int swapIndex = rand.Next(gene.Length);
                    int temp = gene[i];
                    gene[i] = gene[swapIndex];
                    gene[swapIndex] = temp;
                }
            }
        }

        public override ErrorOr<Schedule> Solve()
        {
            List<int[]> population = InitialisePopulation();
            BestMakespan = JobShopObj.BuildGene(population[0]);
            BestGene = population[0];
            int sinceLastImprovement = 0;

            // Solver loop
            for (int generation = 0; generation < _maxGenerations; generation++)
            {
                // Sends the fitness to dict to improve effiancy 
                var fitness = population.ToDictionary(
                    gene => gene,
                    gene => JobShopObj.BuildGene(gene)
                 );

                if (sinceLastImprovement >= _maxNoImprovement) // Early stopping if no improvement
                {
                    break;
                }

                // Sorts the population based on fitness (makespan)
                population.Sort((a, b) => fitness[a].CompareTo(fitness[b]));

                // Update global best solution
                if (JobShopObj.EvaluateGene(population[0]) < BestMakespan)
                {
                    BestMakespan = fitness[population[0]];
                    BestGene = population[0];
                    sinceLastImprovement = 0;
                }

                // Using elitism to keep the best solutions
                List<int[]> newPopulation = new List<int[]>();

                for (int i = 0; i < _elitismCount; i++)
                {
                    newPopulation.Add(population[i]);
                }

                // Parallel processing for offspring generation
                var threadLocalShop = new ThreadLocal<JobShop>(() => JobShopObj.Clone());

                Parallel.For(0, (_populationSize - _elitismCount) / 2, i =>
                {
                    JobShop threadSafeJobShop = threadLocalShop.Value;

                    int[] parent1 = TournamentSelection(population, threadSafeJobShop, fitness);
                    int[] parent2 = TournamentSelection(population, threadSafeJobShop, fitness);

                    (int[] child1, int[] child2) = Crossover(parent1, parent2);

                    Mutate(ref child1);
                    Mutate(ref child2);

                    lock (newPopulation)
                    {
                        newPopulation.Add(child1);
                        newPopulation.Add(child2);
                    }
                });

                threadLocalShop.Dispose();

                population = newPopulation;
                sinceLastImprovement++;
            }

            return Schedule.Create(JobShopObj, BestGene);
        }
    }
}