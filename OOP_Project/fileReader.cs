using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    // Interface for future implementation of file types e.g. JSON
    public interface IFileReader
    {
        public ErrorOr<string[,]> ReadFile(string filePath);
    }

    public class CSVReader : IFileReader
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

                if (lines.Length < 2) {
                    return Error.NotFound(description: "File does not contain valid data");
                }

                // Gets the headers and validates them against the header dictionary
                string[] headers = lines[0].Split(',');
                ErrorOr<int[]> headerIndexs = headerOrder(headers);
                if (headerIndexs.IsError) { return headerIndexs.Errors; }

                string[,] data = new string[lines.Length, headerIndexs.Value.Length];

                // Map header names to the first row of the data array
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
                if (!Globals.HeaderDict.ContainsKey(trimmed))
                {
                    return Error.NotFound(description: $"Header '{trimmed}' is not recognized.");
                }

                indexArray[index] = Globals.HeaderDict[trimmed];
                index++;
            }

            return indexArray;
        }
    }
}