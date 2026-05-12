using ErrorOr;
using OOP_Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project.nUnitTests
{
    internal class JobTests
    {
        [Test]
        public void TestCreateJob()
        {
            ErrorOr<Job> validIDJob = Job.Create(1);
            ErrorOr<Job> invalidIDJob = Job.Create(-1);

            Assert.That(validIDJob.IsError ==  false &&  invalidIDJob.IsError == true);
        }
    }
}
