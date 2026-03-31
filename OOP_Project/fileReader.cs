using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;
using static OOP_Project.Dictionaries;

namespace OOP_Project
{
    interface IFileReader
    {
        public ErrorOr<int[]> ReadFile(string filePath);
    }


    class CSVReader : IFileReader
    {
        public ErrorOr<int[]> ReadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return Error.NotFound("File not found.");
                }

                // Clear the subdivision dictionary before reading a new file to avoid conflicts with previous data
                Dictionaries.subdivisionDict.Clear();

                StreamReader reader = new StreamReader(filePath);

                var lines = File.ReadAllLines("data.csv");




            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }

        private ErrorOr<int[]> headerOrder(string[] headers)
        {
            int[] indexArray = new int[headers.Length];
            int index = 0;

            foreach (string header in headers)
            {
                string trimmed = header.Trim().ToLower();

                if (Dictionaries.headerDict.ContainsKey(trimmed))
                {
                    return Error.NotFound($"Header '{trimmed}' is not recognized.");
                }

                indexArray[index] = Dictionaries.headerDict[trimmed];
            }

            return indexArray;
        }
    }
}


