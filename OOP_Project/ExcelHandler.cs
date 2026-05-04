using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace OOP_Project
{
    internal static class ExcelHandler
    {
        private static int _tableStartRow = 3;
        private static string _headerColour = "D3D3D3"; // Light gray for headers
        private static Dictionary<int, string> _colourDictionary = new()
            {
                { 0, "FFFFFF" }, // White for empty cells
                { 1, "FF0000" }, // Red for Job 1
                { 2, "00FF00" }, // Green for Job 2
                { 3, "0000FF" }, // Blue for Job 3
                { 4, "FFFF00" }, // Yellow for Job 4
                { 5, "FF00FF" }, // Magenta for Job 5
                { 6, "00FFFF" }, // Cyan for Job 6
                { 7, "C0C0C0" }, // Silver for Job 7
                { 8, "800000" }, // Maroon for Job 8
                { 9, "008000" }  // Dark Green for Job 9
            };

        public static void ExportScheduleToExcel(Schedule schedule, string folderPath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Schedule");

                string[] header = schedule.GetHeaders();
                string[,] data = schedule.GetSchedule();

                // Adds the makespan in the top left cell
                worksheet.Cell(1, 1).Value = $"Makespan: {schedule.GetMakespan()} hours";

                // Add Headers
                for (int col = 0; col < header.Length; col++)
                {
                    worksheet.Cell(_tableStartRow, col + 1).Value = header[col];
                    worksheet.Cell(_tableStartRow, col + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(_headerColour);
                    worksheet.Column(col+1).Width = 20;
                }

                // Add Data
                for (int row = 0; row < data.GetLength(0); row++)
                {
                    for (int col = 0; col < data.GetLength(1); col++)
                    {
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Value = data[row, col];
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(GetColour(data[row, col]));
                    }
                }

                // Save the workbook
                string filePath = Path.Combine(folderPath, $"Schedule_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                workbook.SaveAs(filePath);
            }
        }

        private static string GetColour(string cellValue)
        {
            // Get the job id
            string[] split = cellValue.Split(new string[] { " - " }, StringSplitOptions.None);
            if (split.Length == 1)
            {
                return _headerColour;
            }
            int jobID = int.Parse(split[0].Replace("Job ", ""));

            // use mod to cycle through the colour dictionary if there are more jobs than colours
            int colourIndex = jobID % _colourDictionary.Count;

            return _colourDictionary[colourIndex];
        }
    }
}
