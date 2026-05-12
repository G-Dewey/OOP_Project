using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    public class Operation : IDeepCloneable<Operation>
    {
        private int _operationID { get; init; }
        private int _parentJobID { get; init; }
        private string _machine { get; init; }
        private int _processTime { get; init; }

        private Operation(int givenID, string givenMachine, int givenProcessTime, int parentID)
        {
            _operationID = givenID;
            _parentJobID = parentID;
            _machine = givenMachine;
            _processTime = givenProcessTime;
        }

        // Factory method with validation for creating valid operation instances
        public static ErrorOr<Operation> Create(int givenID, string givenMachine, int givenProcessTime, int parentID)
        {
            if (givenID < 0)
            {
                return Error.Validation(description: "Operation ID cannot be negative.");
            }
            if (givenProcessTime <= 0)
            {
                return Error.Validation(description: "Process time must be greater than zero.");
            }
            if (string.IsNullOrEmpty(givenMachine))
            {
                return Error.Validation(description: "Machine cannot be null or empty.");
            }
            if (parentID < 0)
            {
                return Error.Validation(description: "Parent Job ID cannot be negative.");
            }

            return new Operation(givenID, givenMachine, givenProcessTime, parentID);
        }

        public string GetTitle()
        {
            return $"Job: {_parentJobID} | Operation: {_operationID})";
        }

        // Returns a formatted name for schedule identification
        public string GetName()
        {
            return $"Job {_parentJobID} - Operation {_operationID}";
        }

        public string GetMachine()
        {
            return _machine;
        }

        public int GetProcessTime()
        {
            return _processTime;
        }

        // Shallow clone since properties are immutable or primitive
        public Operation Clone() => this;
    }
}