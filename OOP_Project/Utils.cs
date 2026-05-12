using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using ErrorOr;

namespace OOP_Project
{
    internal static class Utils
    {
        // Adds a duration in hours to a starting DateTime
        public static DateTime AddDuration(DateTime start, int duration)
        {
            return start.AddMinutes(duration * 60);
        }

        // Retrieves files from a directory filtered by extension with error handling
        public static ErrorOr<string[]> GetFilesInDir(string dir, string extension)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                return Error.Validation(description: $"Directory '{dir}' does not exist or is invalid.");
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                return Error.Validation(description: "Extension must be provided (e.g. \".txt\" or \"txt\").");
            }

            // Normalize extension formatting
            var ext = extension.Trim();
            if (!ext.StartsWith('.')) ext = "." + ext;
            ext = ext.ToLowerInvariant();

            try
            {
                var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
                                     .Where(f => string.Equals(Path.GetExtension(f).ToLowerInvariant(), ext, StringComparison.Ordinal))
                                     .ToArray();

                if (files.Length == 0)
                {
                    return Error.NotFound(description: $"No files with extension '{ext}' found in directory.");
                }
                return files;
            }
            catch (UnauthorizedAccessException)
            {
                return Error.Forbidden("Access to the directory is denied.");
            }
            catch (System.Security.SecurityException)
            {
                return Error.Unexpected("Cannot access the directory given");
            }
        }

        // Converts raw 2D arrays into a System.Data.DataTable for UI rendering
        public static ErrorOr<DataTable> CreateDataTable(string[] headers, string[,] data, bool hori = false)
        {
            // Basic data validation
            if (headers.Length == 0 | data.GetLength(1) == 0)
            {
                return Error.Validation("Invalid data, cannot transform into table");
            }

            if (headers.Length != data.GetLength(1))
            {
                return Error.Validation("Headers do not match the number of data columns");
            }

            DataTable table = new DataTable();

            // Transpose data if horizontal layout is requested
            if (hori)
            {
                table.Columns.Add(headers[0], typeof(string));
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    table.Columns.Add(data[i, 0]);
                }

                for (int i = 1; i < data.GetLength(1); i++)
                {
                    string[] rowData = new string[data.GetLength(0) + 1];
                    rowData[0] = headers[i];
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        rowData[j + 1] = data[j, i];
                    }
                    table.Rows.Add(rowData);
                }
            }
            else // Standard vertical layout
            {
                foreach (string header in headers)
                {
                    table.Columns.Add(header, typeof(string));
                }

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    string[] rowData = new string[data.GetLength(1)];
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        rowData[j] = data[i, j];
                    }
                    table.Rows.Add(rowData);
                }
            }

            return table;
        }

        // Standardizes input strings for consistent comparisons
        public static string StandardiseString(string input)
        {
            return input.Trim().ToLower();
        }

        // Calculates hours elapsed since the global application start time
        public static int CalcTimeSpan(DateTime dt)
        {
            return (int)(dt - Globals.StartTime).TotalHours;
        }

        // Shortens long file paths for display in restricted UI spaces
        public static string TruncatePath(string path, int maxLength)
        {
            if (path.Length <= maxLength) { return path; }
            return "..." + path[^(maxLength - 3)..];
        }

        // Extracts the file name by stripping the directory prefix
        public static string RemovePath(string path, string folder)
        {
            return path.Replace($"{folder}\\", "");
        }
    }
}