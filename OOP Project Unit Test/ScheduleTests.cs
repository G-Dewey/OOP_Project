using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;
using OOP_Project;
using OOP_Project.nUnitTests;

namespace OOP_Project_Unit_Test
{
    internal class ScheduleTests
    {
        [Test]
        public void TestCreateSchedule()
        {
            ErrorOr<Schedule> EORschedule = Schedule.Create(TestData.TinyJobShop,TestData.TinyJobShop.BaseGene.ToArray());
            if (EORschedule.IsError)
            {
                Assert.Fail();
            }

            Schedule schedule = EORschedule.Value;

            // Checks if the makespan and schedule arent empty
            Assert.That(int.Parse(schedule.GetMakespan()) > 0 && schedule.GetSchedule().Length > 0);
        }
    }
}
