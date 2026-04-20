using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project
{
    internal class Schedule
    {
        // Machine name -> Operation name -> Start time
        public Dictionary<string, Dictionary<string, DateTime[]>> ScheduleDict { get; set; } = new();
        public int totalTime { get; set; }

        public void AddMachineToSchedule(string machineName, Dictionary<string, DateTime[]> machineSchedule)
        {
            if (!ScheduleDict.ContainsKey(machineName))
            {
                return;
            }

            ScheduleDict.Add(machineName, machineSchedule);
        }

        // Check if all machines have been checked 

        // Temp method to output the schedule
        public void PrintSchedule()
        {
            if (ScheduleDict.Count == 0)
            {
                Console.WriteLine("Schedule is empty");
                return;
            }

            var machines = ScheduleDict.Keys.ToList();
            Console.WriteLine(string.Join(", ", machines));

            DateTime time = Globals.StartTime;
            List<int> machineIndexes = new();

            foreach (var machine in machines)
            {
                machineIndexes.Add(0);
            }
    }
}
