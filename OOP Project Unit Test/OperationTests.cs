using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project.nUnitTests
{
    internal class OperationTests
    {
        [Test]
        public void TestCreateOperation()
        {
            int jobID = int.Parse(TestData.ValidData[0, 0]);
            int operationID = int.Parse(TestData.ValidData[0,1]);
            string machine = TestData.ValidData[0, 2];
            int processingTime = int.Parse(TestData.ValidData[0, 3]);

            ErrorOr<Operation> validOperation = Operation.Create(operationID, machine, processingTime, jobID);
            ErrorOr<Operation> invalidOperation = Operation.Create(-1, machine, processingTime, jobID);

            Assert.That(validOperation.IsError == false && invalidOperation.IsError == true);
        }
    }
}

