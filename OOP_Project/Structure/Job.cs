using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    class Job
    {
        public int JobID { get; init; }
        public List<Operation> Operations { get; set; }
        public int OperationIndex { get; init; }
        public DateTime AvailableTime { get; set; }

        private Job(int givenID) 
        { 
            JobID = givenID;
            Operations = new List<Operation>();
            AvailableTime = Globals.StartTime;
        }

        public static ErrorOr<Job> Create(int givenID)
        {
            if (givenID < 0)
            {
                return Error.Validation(description: "Job ID cannot be negative.");
            }

            return new Job(givenID);
        }

        public bool addOperation(int id, string machine, int processingTime)
        {
            var operation = Operation.Create(id, machine, processingTime, JobID, Operations.Count+1);
            if (ErrorHandler.CheckError(operation, 'W')) { return false; }

            Operations.Add(operation.Value);
            return true;
        }

        public int operationCount()
        {
            return Operations.Count;
        }
    }
}
