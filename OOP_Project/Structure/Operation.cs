using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    class Operation
    {
        public int OperationID { get; init; }
        public int ParentJobID { get; init; }
        public int LocalID { get; init; }
        public string Machine { get; init; }
        public int ProcessTime { get; init; }
        public bool Executed { get; private set; }

        private Operation(int givenID, string givenMachine, int givenProcessTime, int parentID, int localID)
        {
            OperationID = givenID;
            ParentJobID = parentID;
            LocalID = localID;
            Machine = givenMachine;
            ProcessTime = givenProcessTime;
            Executed = false;
        }

        // Improve validation (dup IDs, null ids)
        public static ErrorOr<Operation> Create(int givenID, string givenMachine, int givenProcessTime, int parentID, int localID)
        {
            if (givenID < 0)
            {
                return Error.Validation(description: "Operation ID cannot be negative.");
            }
            if (givenProcessTime <= 0)
            {
                return Error.Validation(description: "Process time must be greater than zero.");
            }
            if (givenMachine == null || givenMachine == "")
            {
                return Error.Validation(description: "Machine cannot be null or empty.");
            }
            if (parentID < 0)
            {
                return Error.Validation(description: "Parent Job ID cannot be negative.");
            }
            if (localID < 0)
            {
                return Error.Validation(description: "Local ID cannot be negative.");
            }

            return new Operation(givenID, givenMachine, givenProcessTime, parentID, localID);
        }

        public void MarkCompleted()
        {
            Executed = true;
        }

        public string GetTitle()
        {
            return $"Job: {ParentJobID} | Operation: {OperationID})";
        }
    }
}
