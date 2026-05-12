using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OOP_Project
{
    public class JobShop : IDeepCloneable<JobShop>
    {
        private Dictionary<int, Job> _jobs { get; set; }
        private Dictionary<string, Machine> _machines { get; set; }
        private DateTime _startTime { get; set; }
        public List<int> BaseGene { get; set; } = new List<int>();

        // Constructor Overloading - used by Clone method to create an empty instance without processing data
        private JobShop() { }

        private JobShop(string[,] data)
        {
            _jobs = new Dictionary<int, Job> { };
            _machines = new Dictionary<string, Machine> { };

            _startTime = Globals.StartTime;
            ProcessData(data);
        }

        public static ErrorOr<JobShop> Create(string[,] data)
        {
            // Validate data format here (check for correct number of columns, valid types, etc.)
            try
            {
                return new JobShop(data);
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed initailise job shop");
            }
        }

        // Parses raw string data into Job, Machine, and Operation objects
        private void ProcessData(string[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                int jobID = int.Parse(data[i, 0]);
                int operationID = int.Parse(data[i, 1]);
                string subdivision = data[i, 2];
                int processingTime = int.Parse(data[i, 3]);

                // Checks if job already exists, if not creates it and adds to dict
                if (!_jobs.ContainsKey(jobID))
                {
                    var job = Job.Create(jobID);
                    if (ErrorHandler.CheckError(job, 'W')) { continue; }

                    _jobs.Add(jobID, job.Value);
                }

                // Checks if machine already exists, if not creates it and adds to dict
                if (!_machines.ContainsKey(subdivision))
                {
                    var machine = Machine.Create(subdivision);
                    if (ErrorHandler.CheckError(machine, 'C')) { continue; }
                    _machines.Add(subdivision, machine.Value);
                }

                // Adds operation to job, if fails logs error and continues
                if (!_jobs[jobID].addOperation(operationID, subdivision, processingTime))
                {
                    // Send Warning
                    continue;
                }

                this.BaseGene.Add(jobID);
            }
        }

        // Calculates the total duration (makespan) by finding the latest machine end time
        public int FindMakeSpan()
        {
            DateTime end = Globals.StartTime;
            foreach (Machine machine in _machines.Values)
            {
                if (machine.NextAvailable > end)
                {
                    end = machine.NextAvailable;
                }
            }

            return Utils.CalcTimeSpan(end);
        }

        // Evaluates fitness - lightweight simulation to calculate makespan without full scheduling
        public int EvaluateGene(int[] gene)
        {
            foreach (Machine m in _machines.Values) m.Reset();
            foreach (Job j in _jobs.Values) j.Reset();

            foreach (int jobID in gene)
            {
                Job job = _jobs[jobID];
                DateTime jobAvailable = job.GetAvailableTime();
                ErrorOr<Operation> EORoperation = job.NextOperation();

                if (ErrorHandler.CheckError(EORoperation, 'C')) continue;

                Operation operation = EORoperation.Value;
                Machine machine = _machines[operation.GetMachine()];

                // Determine start time based on both machine and job availability
                DateTime start = machine.NextAvailable > jobAvailable
                    ? machine.NextAvailable
                    : jobAvailable;

                DateTime end = start.AddHours(operation.GetProcessTime());
                machine.NextAvailable = end;
                job.SetAvailableTime(end);
            }

            return FindMakeSpan();
        }

        // Used for building a schedule - performs a full simulation and records time slots
        public int BuildGene(int[] gene)
        {
            // Clear the machines schedule and next available time
            foreach (Machine m in _machines.Values) m.Reset();
            foreach (Job j in _jobs.Values) j.Reset();

            Job job;
            DateTime jobAvailable;
            ErrorOr<Operation> EORoperation;
            Operation operation;
            Machine machine;

            foreach (int jobID in gene)
            {
                job = _jobs[jobID];
                jobAvailable = job.GetAvailableTime();
                EORoperation = job.NextOperation();

                if (ErrorHandler.CheckError(EORoperation, 'C'))
                {
                    continue;
                }

                operation = EORoperation.Value;

                machine = _machines[operation.GetMachine()];
                ErrorOr<DateTime> EORendTime = machine.ScheduleOperation(operation.GetName(), operation.GetProcessTime(), jobAvailable);

                if (ErrorHandler.CheckError(EORendTime, 'C'))
                {
                    // Add fail logic
                    return -1;
                }

                job.SetAvailableTime(EORendTime.Value);
            }

            return FindMakeSpan();
        }

        public string[] GetMachineNames()
        {
            return _machines.Keys.ToArray();
        }

        // Checks what operation is running on a specific machine at a specific time
        public ErrorOr<string> CheckMachine(string machineName, DateTime dateTime)
        {
            if (_machines.TryGetValue(machineName, out Machine machine))
            {
                if (machine.LocalSchedule.TryGetValue(dateTime, out string operation))
                {
                    return operation;
                }
                return "";
            }
            else
            {
                return Error.Failure("Could not find machine");
            }
        }

        // Cloning method to create a deep copy of the JobShop instance, useful for solvers to avoid mutating the original data when parallel
        public JobShop Clone()
        {
            return new JobShop
            {
                _jobs = _jobs.ToDictionary(e => e.Key, e => e.Value.Clone()),
                _machines = _machines.ToDictionary(e => e.Key, e => e.Value.Clone()),
                _startTime = _startTime,
                BaseGene = new List<int>(BaseGene)
            };
        }
    }
}