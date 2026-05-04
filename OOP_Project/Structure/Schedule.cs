using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    internal class Schedule
    {
        private string[] _machines { get; set; }
        private List<List<string>> _schedule { get; set; } = new List<List<string>>();
        private int _makespan { get; set; }

        private Schedule(JobShop jobShop, int[] gene)
        {
            _machines = jobShop.GetMachineNames();
            _makespan = jobShop.BuildGene(gene);
            MakeSchedule(jobShop);

        }

        public static ErrorOr<Schedule> Create(JobShop jobShop, int[] gene)
        {
            if (gene == null || gene.Length == 0)
            {
                return Error.Validation(description: "Gene cannot be null or empty.");
            }

            if (jobShop == null)
            {
                return Error.Validation(description: "Job shop can not be null.");
            }

            try
            {
                return new Schedule(jobShop, gene);
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to create schedule: {ex.Message}");
            }
        }

        private void MakeSchedule(JobShop jobShop)
        {
            DateTime time = Globals.StartTime;
            for (int i = 0; i < _makespan; i++)
            {
                List<String> row = new List<string> { time.ToString("dd/MM HH:mm") };

                foreach (string machine in _machines)
                {
                    ErrorOr<string> EORoperation = jobShop.CheckMachine(machine, time);

                    // Improve error handling
                    if (ErrorHandler.CheckError(EORoperation, 'C')) { Debug.Log("Error with getting operation"); return; }

                    row.Add(EORoperation.Value);
                }

                _schedule.Add(row);

                // Increments time
                time = time.AddHours(1);
            }
        }

        public string[,] GetSchedule()
        {
            // Have to convert schedule to matrix
            int rowCount = _schedule.Count;
            int colCount = _schedule.Max(list => list.Count);

            string[,] schedule = new string[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < _schedule[i].Count; j++)
                {
                    schedule[i, j] = _schedule[i][j];
                }
            }

            return schedule;
        }

        public string[] GetHeaders()
        {
            string[] timeHeader = { "Time" };
            return timeHeader.Concat(_machines).ToArray();
        }

        public string GetMakespan()
        {
            return _makespan.ToString();
        }
    }
}
