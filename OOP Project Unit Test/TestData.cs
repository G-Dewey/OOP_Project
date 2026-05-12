using OOP_Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project.nUnitTests
{
    internal class TestData
    {
        public static string[,] ValidData => new string[,]
        {
            { "1", "1", "MachineA", "2" },
            { "1", "2", "MachineB", "3" },
            { "2", "1", "MachineA", "1" },
            { "2", "2", "MachineB", "2" },
        };

        public static string[,] InvalidData => new string[,]
        {
            { "NOT_AN_INT", "1", "MachineA", "2" },
        };

        public static JobShop TinyJobShop = JobShop.Create(ValidData).Value;

    }
}
