using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using ErrorOr;  

namespace OOP_Project
{
    public class FrontendHandler : Window
    {
        private JobShop JobShop;
        private string DirectoryPath = "jobs";
        private string[,] JobData;
        private string FullPath;
        private string FileName;
        private string SelectedAlgorithm;

        public FrontendHandler()
        {
            // Set up basic window properties
            Title = "WestEng Job Shop Scheduler";
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        public void RunApp()
        {
            Application.Init();

            // Add this handler instance to the Top level
            Application.Top.Add(this);

            // Trigger the first "page"
            JobSelectionPage();

            Application.Run();
            Application.Shutdown();
        }

        // ERROR HANDLING METHODS 
        // -------------------------------------------------
        // Returns the program to last safe state
        private void CriticalError(string message)
        {
            // Add logic to revert to last safe stage 
            MessageBox.ErrorQuery("Critical Error", message, "OK");
        }

        // Returns the program to the selection stage
        private void SelectionError(string message)
        {
            MessageBox.ErrorQuery("Error", message, "OK");
        }

        // Displays a warning banner
        private void Warning(string message)
        {
            // Change to display a warning banner 
            MessageBox.Query("Warning", message, "OK");
        }

        /*
         C - Critical Error: A server issue that will return the algorithm to it's nearest safe state
         S - Selection Error: Issue with data/option selected, will return the algorithm to the selection stage
         W - Warning: A non critical issue that will not affect the algorithm but should be noted
         O - Other: An error that doesn't fit the other categories it will be handled on a case by case 
        */
        private bool CheckError(IErrorOr item, char severity)
        {
            Debug.Log($"{item.IsError}");
            if (item.IsError)
            {
                var errorMessage = string.Join(", ", item.Errors.Select(e => e.Description));
                switch (severity)
                {
                    case 'C':
                        CriticalError(errorMessage);
                        Debug.Log($"Critical error occurred: {errorMessage}");
                        return true;
                    case 'W':
                        Warning(errorMessage);
                        Debug.Log($"Warning occurred: {errorMessage}");
                        return true;
                    case 'S':
                        SelectionError(errorMessage);
                        Debug.Log($"Selection error occurred: {errorMessage}");
                        return true;
                    case 'O':
                        Debug.Log($"Other error occurred: {errorMessage}");
                        return true;
                    default:
                        Debug.Log($"Unknown error severity: {severity}");
                        return true;
                }
            }
            return false;
        }

        // UTIL METHODS
        // -------------------------------------------------

        private ErrorOr<string[,]> ReadJobFile(string fileType)
        {
            IFileReader reader;

            // TODO INCLUDE OTHERFILE TYPES
            if (fileType == "csv")
            {
                reader = new CSVReader();
            }
            else
            {
                return Error.Unexpected(description: "Unsupported file type");
            }

            return reader.ReadFile(FileName);
        }

        public bool CreateJobShop()
        {
            ErrorOr<JobShop> jobShopResult = JobShop.Create(JobData);

            if (CheckError(jobShopResult, 'C'))
            {
                return false;
            }

            JobShop = jobShopResult.Value;

            Debug.Log($"Successfully created JobShop");

            return true;
        }

        // PAGE METHODS
        // -------------------------------------------------

        public void JobSelectionPage()
        {
            this.RemoveAll();

            var titleLabel = new Label("Welcome to the WestEng Job Shop Scheduler")
            {
                X = Pos.Center(),
                Y = 1,
            };

            var filesFrame = new FrameView("Available Job Files")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(60),
                Height = Dim.Percent(60)
            };

            ErrorOr<string[]> fileResult = Utils.GetFilesInDir(DirectoryPath, "csv");

            Debug.Log($"File result: {fileResult.IsError}, {fileResult.Value?.Length}");

            if (CheckError(fileResult, 'O'))
            {
                var errorText = string.Join(Environment.NewLine, fileResult.Errors.Select(e => e.Description));
                filesFrame.Add(new Label(errorText) { X = 0, Y = 0, ColorScheme = Colors.Error });
            }
            else
            {
                var listView = new ListView(fileResult.Value)
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                listView.OpenSelectedItem += (args) =>
                {
                    listView.OpenSelectedItem += (args) =>
                    {
                        FileName = args.Value.ToString();

                        FullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(DirectoryPath, FileName));

                        // 3. Pass the path to your processing function
                        Debug.Log($"Selected file: {FullPath}");

                        ErrorOr<string[,]> FileData = ReadJobFile("csv");

                        if (!CheckError(FileData, 'C'))
                        {
                            JobData = FileData.Value;
                            Debug.Log($"Successfully read file: {FileName} : {JobData}");

                            CreateJobShop();
                            SelectAlgorithmPage();
                        }
                    };
                };

                filesFrame.Add(listView);
            }

            this.Add(titleLabel, filesFrame);
        }

        private void SelectAlgorithmPage()
        {
            this.RemoveAll();

        }
    }
}
