using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using ErrorOr;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project.nUnitTests
{
    internal class FileReaderTests
    {
        [Test]
        public void TestCSVReader()
        {
            IFileReader CSVReader = new CSVReader();
            /// FIX IN MORNING
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            ErrorOr<string[,]> validData = CSVReader.ReadFile("C:\\Users\\phild\\OneDrive\\Documents\\University Work\\Year 2\\OOP V2\\OOP Project Unit Test\\test_data.csv");
            ErrorOr<string[,]> invalidData = CSVReader.ReadFile(appDir + "notReal.json");
            ErrorOr<string[,]> brokenData = CSVReader.ReadFile("C:\\Users\\phild\\OneDrive\\Documents\\University Work\\Year 2\\OOP V2\\OOP Project Unit Test\\test_invalid_data.csv");

            if (validData.IsError == true || invalidData.IsError == false || brokenData.IsError == false)
            {
                Console.WriteLine(validData.Errors.Select(e => e.Description));
                Assert.Fail();
            }

            string[,] data = validData.Value;

            Assert.That(data.Length > 0);
        }
    }
}
