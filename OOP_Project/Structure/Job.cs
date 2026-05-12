using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    public class Job : IDeepCloneable<Job>
    {
        private int _jobID { get; init; }
        private List<Operation> _operations { get; set; }
        private int _operationIndex { get; set; }
        private DateTime _availableTime { get; set; }

        private Job(int givenID)
        {
            _jobID = givenID;
            _operations = new List<Operation>();
            _availableTime = Globals.StartTime;
            _operationIndex = 0;
        }

        // Resets the job state for a new simulation run
        public void Reset()
        {
            _availableTime = Globals.StartTime;
            _operationIndex = 0;
        }

        public static ErrorOr<Job> Create(int givenID)
        {
            if (givenID < 0)
            {
                return Error.Validation(description: "Job ID cannot be negative.");
            }

            return new Job(givenID);
        }

        // Creates and attaches a new operation to this job
        public bool addOperation(int id, string machine, int processingTime)
        {
            var operation = Operation.Create(id, machine, processingTime, _jobID);
            if (ErrorHandler.CheckError(operation, 'W')) { return false; }

            _operations.Add(operation.Value);
            return true;
        }

        public int operationCount()
        {
            return _operations.Count;
        }

        // Retrieves the next sequential operation and increments the pointer
        public ErrorOr<Operation> NextOperation()
        {
            if (_operationIndex < 0 || _operationIndex >= _operations.Count)
            {
                return Error.Validation(description: $"Operation index {_operationIndex} is out of range.");
            }

            Operation op = _operations[_operationIndex];
            _operationIndex++;
            return op;
        }

        public DateTime GetAvailableTime()
        {
            return _availableTime;
        }

        public void SetAvailableTime(DateTime newTime)
        {
            _availableTime = newTime;
        }

        // Deep clone to ensure independent state during parallel solving
        public Job Clone()
        {
            return new Job(this._jobID)
            {
                _operations = _operations.Select(op => op.Clone()).ToList(),
                _availableTime = this._availableTime,
                _operationIndex = this._operationIndex
            };
        }
    }
}