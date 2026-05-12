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
        private static int _tableStartRow = 1;
        private static int _tableEndRow = _tableStartRow;
        private static string _headerColour = "FFFFFF"; // white for headers
        private static Dictionary<int, string> _colourDictionary = new()
            {
                { 0, "FFCC80" }, // Orange for Job 0
                { 1, "EF9A9A" }, // Red for Job 1
                { 2, "C8E6C9" }, // Green for Job 2
                { 3, "BBDEFB" }, // Blue for Job 3
                { 4, "FFF59D" }, // Yellow for Job 4
                { 5, "E1BEE7" }, // Magenta for Job 5
                { 6, "B2DFDB" }, // Cyan for Job 6
                { 7, "D7CCC8" }, // Silver for Job 7
                { 8, "E57373" }, // Maroon for Job 8
                { 9, "A5D6A7" }  // Dark Green for Job 9
            };

        // Main entry point for exporting the generated schedule to an Excel file
        public static void ExportScheduleToExcel(Schedule schedule, string folderPath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Schedule");

                string[] header = schedule.GetHeaders();
                string[,] data = schedule.GetSchedule();

                // Add Headers with styling
                for (int col = 0; col < header.Length; col++)
                {
                    worksheet.Cell(_tableStartRow, col + 1).Value = header[col];
                    worksheet.Cell(_tableStartRow, col + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(_headerColour);
                    worksheet.Cell(_tableStartRow, col + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    worksheet.Cell(_tableStartRow, col + 1).Style.Border.InsideBorder = XLBorderStyleValues.Medium;
                    worksheet.Cell(_tableStartRow, col + 1).Style.Font.Bold = true;
                    worksheet.Cell(_tableStartRow, col + 1).Style.Font.FontSize = 14;
                    worksheet.Cell(_tableStartRow, col + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(_tableStartRow, col + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Column(col + 1).Width = 20;
                }

                worksheet.Row(_tableStartRow).Height = 30;
                _tableEndRow++;

                // Add Data rows with color-coding per Job ID
                for (int row = 0; row < data.GetLength(0); row++)
                {
                    for (int col = 0; col < data.GetLength(1); col++)
                    {
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Value = data[row, col];
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(GetColour(data[row, col]));
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row + _tableStartRow + 1, col + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        if (col == 0)
                        {
                            worksheet.Cell(row + _tableStartRow, col + 1).Style.Font.Bold = true;
                        }
                    }
                    worksheet.Row(row + _tableStartRow + 1).Height = 20;
                    _tableEndRow++;
                }

                // Add a footer row showing the total Makespan
                worksheet.Row(_tableEndRow).Height = 30;
                var makespanRange = worksheet.Range(_tableEndRow, 1, _tableEndRow, data.GetLength(1));
                makespanRange.Merge();
                makespanRange.Value = $"Makespan: {schedule.GetMakespan()} Hours";
                makespanRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                makespanRange.Style.Border.InsideBorder = XLBorderStyleValues.Medium;
                makespanRange.Style.Font.Bold = true;
                makespanRange.Style.Font.FontSize = 14;
                makespanRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                makespanRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Save the workbook with a timestamped filename
                string filePath = Path.Combine(folderPath, $"Schedule_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                workbook.SaveAs(filePath);
            }
        }

        // Determines cell background color based on the Job ID found in the text
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