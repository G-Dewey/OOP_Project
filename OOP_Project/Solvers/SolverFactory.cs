using ErrorOr;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OOP_Project
{
    internal class SolverFactory
    {
        private readonly Dictionary<string, Func<JobShop, Solver>> _registry = new Dictionary<string, Func<JobShop, Solver>>();

        public SolverFactory()
        {
            _registry.Add("random", (jobShop) => new RandomShuffleSolver(jobShop));
            _registry.Add("greedy hill climber", (jobShop) => new GreedySolver(jobShop));
            _registry.Add("genetic", (jobShop) => new GeneticSolver(jobShop));
        }

        public string[] GetAvailableAlgorithms()
        {
            string[] algorithms = _registry.Keys
                    .Select(k => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(k.ToLower()))
                    .ToArray();

            return algorithms;
        }

        public ErrorOr<Solver> Create(string solverName, JobShop jobShop)
        {
            solverName = solverName.Trim().ToLower();

            // ERROR CHECKING
            if (!_registry.ContainsKey(solverName))
            {
                return Error.NotFound($"Solver '{solverName}' not found.");
            }

            if (jobShop == null)
            {
                return Error.NotFound($"JobShop instance cannot be null.");
            }

            _registry.TryGetValue(solverName, out Func<JobShop, Solver> solverFactory);

            return solverFactory(jobShop);
        }
    }
}
