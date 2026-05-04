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
    internal class JobShop
    {
        private Dictionary<int, Job> Jobs { get; set; }
        private Dictionary<string, Machine> Machines { get; set; }
        private DateTime StartTime { get; set; }

        // Temp public
        public List<int> BaseGene { get; set; } = new List<int>();

        private JobShop(string[,] data)
        {
            Jobs = new Dictionary<int, Job> { };
            Machines = new Dictionary<string, Machine> { };

            StartTime = Globals.StartTime;
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

        private void ProcessData(string[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                int jobID = int.Parse(data[i, 0]);
                int operationID = int.Parse(data[i, 1]);
                string subdivision = data[i, 2];
                int processingTime = int.Parse(data[i, 3]);

                // Checks if job already exists, if not creates it and adds to dict
                if (!Jobs.ContainsKey(jobID))
                {
                    var job = Job.Create(jobID);
                    if (ErrorHandler.CheckError(job, 'W')) { continue; }

                    Jobs.Add(jobID, job.Value);
                }

                // Checks if machine already exists, if not creates it and adds to dict
                if (!Machines.ContainsKey(subdivision))
                {
                    var machine = Machine.Create(subdivision);
                    if (ErrorHandler.CheckError(machine, 'C')) { continue; }
                    Machines.Add(subdivision, machine.Value);
                }

                // Adds operation to job, if fails logs error and continues
                if (!Jobs[jobID].addOperation(operationID, subdivision, processingTime))
                {
                    // Send Warning
                    continue;
                }

                this.BaseGene.Add(jobID);
            }
        }

        private int[] ShuffleGene()
        {
            Random rand = new Random();
            return BaseGene.OrderBy(x => rand.Next()).ToArray();
        }

        // TESTER FUNCTION TO PRINT THE BASE GENE, CAN BE REMOVED LATER
        public void PrintShuffledGene()
        {
            var shuffledGene = ShuffleGene();
            Console.WriteLine("Shuffled Gene:");
            foreach (var jobID in shuffledGene)
            {
                Console.Write($"{jobID} ");
            }
            Console.WriteLine();
        }

        public void PrintGene()
        {
            Console.WriteLine("Base Gene:");
            foreach (var jobID in BaseGene)
            {
                Console.Write($"{jobID} ");
            }
            Console.WriteLine();
        }

        public int FindMakeSpan()
        {
            DateTime end = Globals.StartTime;
            foreach (Machine machine in Machines.Values)
            {
                if (machine.NextAvailable > end)
                {
                    end = machine.NextAvailable;
                }
            }

            return Utils.CalcTimeSpan(end);
        }

        public int BuildGene(int[] gene)
        {
            // Clear the machines schedule and next available time
            foreach (Machine m in Machines.Values)
            {
                m.Reset();
            }

            foreach (Job j in Jobs.Values)
            {
                j.Reset();
            }

            Job job;
            DateTime jobAvailable;
            ErrorOr<Operation> EORoperation;
            Operation operation;
            Machine machine;

            foreach (int jobID in gene)
            {
                job = Jobs[jobID];
                jobAvailable = job.GetAvailableTime();
                EORoperation = job.NextOperation();

                if (ErrorHandler.CheckError(EORoperation, 'C'))
                {
                    continue;
                }

                operation = EORoperation.Value;

                machine = Machines[operation.Machine];
                ErrorOr<DateTime> EORendTime = machine.ScheduleOperation(operation.GetName(), operation.ProcessTime, jobAvailable);

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
            return Machines.Keys.ToArray();
        }

        public ErrorOr<string> CheckMachine(string machineName, DateTime dateTime)
        {
            if (Machines.TryGetValue(machineName, out Machine machine))
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
    }
}
