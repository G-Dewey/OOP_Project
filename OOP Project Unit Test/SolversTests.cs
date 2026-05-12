using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project.nUnitTests;

internal class SolversTests
{
    [Test]
    public void TestSolverFactory()
    {
        SolverFactory solverFactory = new SolverFactory();

        ErrorOr<Solver> validSolver = solverFactory.Create("genetic", TestData.TinyJobShop);
        ErrorOr<Solver> invalidSolver = solverFactory.Create("This is not a solver", TestData.TinyJobShop);

        Assert.That(validSolver.IsError == false &&  invalidSolver.IsError == true);
    }

    [Test]
    public void TestSolvers()
    {
        SolverFactory solverFactory = new SolverFactory();

        ErrorOr<Solver> EORrandomSolver = solverFactory.Create("random", TestData.TinyJobShop);
        ErrorOr<Solver> EORhillClimberSolver = solverFactory.Create("greedy hill climber", TestData.TinyJobShop);
        ErrorOr<Solver> EORgeneticSolver = solverFactory.Create("genetic", TestData.TinyJobShop);

        if (EORrandomSolver.IsError || EORhillClimberSolver.IsError || EORgeneticSolver.IsError)
        {
            Assert.Fail();
        }

        List<Solver> solvers = new List<Solver>();

        solvers.Add(EORrandomSolver.Value);
        solvers.Add(EORhillClimberSolver.Value);
        solvers.Add(EORgeneticSolver.Value);

        foreach (Solver solver in solvers)
        {
            ErrorOr<Schedule> EORSchedule = solver.Solve();
            if (EORSchedule.IsError)
            {
                Console.WriteLine($"{solver.GetSolverName()} Failed");
                Assert.Fail();
            }

            Schedule schedule = EORSchedule.Value;

            if (int.Parse(schedule.GetMakespan()) < 0 || schedule.GetSchedule().Length == 0)
            {
                Console.WriteLine($"{solver.GetSolverName()} Failed");
                Assert.Fail();
            }
        }

        Assert.Pass();
    }
}
