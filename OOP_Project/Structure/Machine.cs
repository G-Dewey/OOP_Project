using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    internal class Machine
    {
        public string Name { get; init; }
        public Dictionary<string, DateTime[]> Schedule { get; set; } = new();
        public DateTime NextAvailable { get; set; }

        private Machine(string name)
        {
            Name = name;
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

        // Returns false if error, true if successful
        public ErrorOr<DateTime> ScheduleOperation(string operationName, int duration, DateTime jobAvailable)
        {
            if (Schedule.ContainsKey(operationName))
            {
                return Error.Validation(description: $"Operation {operationName} is already scheduled on machine {Name}.");
            }

            try
            {
                DateTime operationTime = NextAvailable > jobAvailable ? NextAvailable : jobAvailable;
                DateTime endTime = Utils.AddDuration(operationTime, duration);

                Schedule.Add(operationName, new[] { operationTime, endTime });

                this.NextAvailable = Utils.AddDuration(operationTime, duration);

                // Returns the end time so that the job can update its available time for the next operation
                return endTime;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to schedule operation {operationName} on machine {Name}: {ex.Message}");
            }
        }
    }
}
