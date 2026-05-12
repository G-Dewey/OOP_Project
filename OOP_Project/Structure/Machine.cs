using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    class Machine : IDeepCloneable<Machine>
    {
        private string _name { get; init; }
        public Dictionary<DateTime, string> LocalSchedule { get; set; } = new();
        public DateTime NextAvailable { get; set; }

        private Machine(string name)
        {
            _name = name;
            NextAvailable = Globals.StartTime;
        }

        public static ErrorOr<Machine> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Error.Validation(description: "Machine name cannot be empty.");
            }

            return new Machine(name);
        }

        // Clears current schedule and resets availability to system start time
        public void Reset()
        {
            LocalSchedule.Clear();
            NextAvailable = Globals.StartTime;
        }

        // Returns returns the end time of the operation so that the job can update its available time for the next operation
        public ErrorOr<DateTime> ScheduleOperation(string operationName, int duration, DateTime jobAvailable)
        {
            // Prevent duplicate scheduling of the same operation
            if (LocalSchedule.ContainsValue(operationName))
            {
                return Error.Validation(description: $"Operation {operationName} is already scheduled on machine {_name}.");
            }

            try
            {
                // Start time is the later of: when the machine is free OR when the job arrives
                DateTime operationTime = NextAvailable > jobAvailable ? NextAvailable : jobAvailable;

                // Fill the schedule dictionary for each hour of duration
                for (int i = 0; i < duration; i++)
                {
                    LocalSchedule.Add(operationTime, operationName);
                    operationTime = operationTime.AddHours(1);
                }

                // by the end of the loop, operationTime will be the end time of the operation, so we set NextAvailable to this time
                this.NextAvailable = operationTime;

                // Returns the end time so that the job can update its available time for the next operation
                return operationTime;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to schedule operation {operationName} on machine {_name}: {ex.Message}");
            }
        }

        // Creates a deep copy of the machine's state for parallel processing
        public Machine Clone()
        {
            Machine clone = new Machine(this._name)
            {
                LocalSchedule = new Dictionary<DateTime, string>(this.LocalSchedule),
                NextAvailable = this.NextAvailable
            };
            return clone;
        }
    }
}