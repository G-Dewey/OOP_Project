using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    internal class JobShop
    {
        private Dictionary<int, Job> Jobs { get; set; }
        private Dictionary<string, Machine> Machines { get; set; }
        private DateTime StartTime { get; set; }
        private List<int> BaseGene { get; set; } = new List<int>();

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
            return new JobShop(data);
        }

        private void ProcessData(string[,] data)
        {
            for(int i = 0; i < data.GetLength(0); i++)
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
                    Console.WriteLine($"Failed to add operation {operationID} to job {jobID}");
                }

                this.BaseGene.Add(jobID);
            }

            Console.WriteLine($"Processed {data.GetLength(0)} rows of data.");
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

        public 
    }
}
