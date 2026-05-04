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
        public ErrorOr<string[,]> ReadFile(string filePath);
    }


    class CSVReader : IFileReader
    {
        public ErrorOr<string[,]> ReadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return Error.NotFound(description: "File not found");
                }

                var lines = File.ReadAllLines(filePath);

                // Gets the headers and validates them against the header dictionary
                string[] headers = lines[0].Split(',');
                ErrorOr<int[]> headerIndexs = headerOrder(headers);
                if (headerIndexs.IsError){ return headerIndexs.Errors; }

                string[,] data = new string[lines.Length, headerIndexs.Value.Length];

                for (int i = 0; i < headerIndexs.Value.Length; i++)
                {
                    data[0, i] = headers[headerIndexs.Value[i]];
                }

                // Reads each line (exluding the header). Reorders the values according to the header order and inputs them into data
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');
                    for (int j = 0; j < headerIndexs.Value.Length; j++)
                    {
                        data[i, headerIndexs.Value[j]] = values[j];
                    }
                }

                return data;

            }
            catch (Exception ex)
            {
                return Error.Unexpected(description: ex.Message);
            }
        }


        // Checks each header against the dictionary and returns an array of the corresponding indices.
        // Used to determine the order of the headers.
        private ErrorOr<int[]> headerOrder(string[] headers)
        {
            int[] indexArray = new int[headers.Length];
            int index = 0;

            foreach (string header in headers)
            {
                string trimmed = header.Trim().ToLower();

                // Check if the header is recognized. If not, return an error.
                if (!Dictionaries.HeaderDict.ContainsKey(trimmed))
                {
                    return Error.NotFound(description: $"Header '{trimmed}' is not recognized.");
                }

                indexArray[index] = Dictionaries.HeaderDict[trimmed];
                index++;
            }

            return indexArray;
        }
    }
}


