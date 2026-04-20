// See https://aka.ms/new-console-template for more information
using ErrorOr;
using System;

namespace OOP_Project
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Temp 
            IFileReader reader = new CSVReader();

            ErrorOr<String[,]> data = reader.ReadFile("jobs/jobs_small.csv");

            // Error Handelling for the reader
            if (data.IsError)
            {
                // Output the errors
                foreach (var error in data.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                }
            }

            // Temp for outputting the data read from the file
            for (int i = 0; i < data.Value.GetLength(0); i++)
            {
                for (int j = 0; j < data.Value.GetLength(1); j++)
                {
                    Console.Write($"{data.Value[i, j]} ");
                }
                Console.WriteLine($" i: {i}");
            }

            ErrorOr<JobShop> jobShopObj = JobShop.Create(data.Value);

            if (ErrorHandler.CheckError(jobShopObj, 'C'))
            {
                Console.WriteLine("Failed to create JobShop object.");
                return;
            }

            JobShop jobShop = jobShopObj.Value;

            jobShop.PrintShuffledGene();
        }
    }
}