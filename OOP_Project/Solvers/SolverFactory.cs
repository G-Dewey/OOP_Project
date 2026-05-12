using ErrorOr;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OOP_Project
{
    public class SolverFactory
    {
        // Maps algorithm names to constructor delegates
        private readonly Dictionary<string, Func<JobShop, Solver>> _registry = new Dictionary<string, Func<JobShop, Solver>>();

        public SolverFactory()
        {
            _registry.Add("random", (jobShop) => new RandomShuffleSolver(jobShop));
            _registry.Add("greedy hill climber", (jobShop) => new HillClimberSolver(jobShop));
            _registry.Add("genetic", (jobShop) => new GeneticSolver(jobShop));
        }

        // Returns formatted list of solver names for UI display
        public string[] GetAvailableAlgorithms()
        {
            return _registry.Keys
                    .Select(k => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(k.ToLower()))
                    .ToArray();
        }

        public ErrorOr<Solver> Create(string solverName, JobShop jobShop)
        {
            solverName = solverName.Trim().ToLower();

            // Validation checks
            if (!_registry.ContainsKey(solverName))
                return Error.NotFound($"Solver '{solverName}' not found.");

            if (jobShop == null)
                return Error.Validation("JobShop instance cannot be null.");

            // Resolve and return the requested solver
            _registry.TryGetValue(solverName, out var solverFactory);
            return solverFactory!(jobShop);
        }
    }
}