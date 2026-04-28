using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using ErrorOr;
using Terminal.Gui.Graphs;
using System.Data;

namespace OOP_Project
{
    public class FrontendHandler : Window
    {
        private JobShop _jobShop;
        private string _directoryPath = "jobs";
        private string[] _dataHeaders;
        private string[,] _jobData;
        private string _fullPath;
        private string _fileName;
        private string _selectedAlgorithm;
        private Solver _solver;

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
            //Debug.Log($"{item.IsError}");
            if (item.IsError)
            {
                var errorMessage = string.Join(", ", item.Errors.Select(e => e.Description));
                switch (severity)
                {
                    case 'C':
                        CriticalError(errorMessage);
                        //Debug.Log($"Critical error occurred: {errorMessage}");
                        return true;
                    case 'W':
                        Warning(errorMessage);
                        //Debug.Log($"Warning occurred: {errorMessage}");
                        return true;
                    case 'S':
                        SelectionError(errorMessage);
                        //Debug.Log($"Selection error occurred: {errorMessage}");
                        return true;
                    case 'O':
                        //Debug.Log($"Other error occurred: {errorMessage}");
                        return true;
                    default:
                        //Debug.Log($"Unknown error severity: {severity}");
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

            return reader.ReadFile(_fileName);
        }

        public bool CreateJobShop()
        {
            ErrorOr<JobShop> jobShopResult = JobShop.Create(_jobData);

            if (CheckError(jobShopResult, 'C'))
            {
                return false;
            }

            _jobShop = jobShopResult.Value;

            //Debug.Log($"Successfully created JobShop");

            return true;
        }

        // PAGE METHODS
        // -------------------------------------------------

        public void JobSelectionPage()
        {
            this.RemoveAll();

            var filesFrame = new FrameView("Available Job Files")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(60),
                Height = Dim.Percent(60)
            };

            // Position the title using Pos.Top(filesFrame) and an integer offset (Pos - int is supported)
            var titleLabel = new Label("Welcome to the WestEng Job Shop Scheduler")
            {
                X = Pos.Center(),
                Y = Pos.Top(filesFrame) - 3,
            };

            ErrorOr<string[]> fileResult = Utils.GetFilesInDir(_directoryPath, "csv");

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
                    // args.Value will be the selected item (string path in this case)
                    _fileName = args.Value.ToString();
                    _fullPath = System.IO.Path.GetFullPath(_fileName);

                    ErrorOr<string[,]> FileData = ReadJobFile("csv");

                    if (!CheckError(FileData, 'C'))
                    {
                        // Splits data into headers and the rest of data for easier handling later on
                        _dataHeaders = Enumerable.Range(0, FileData.Value.GetLength(1))
                                      .Select(x => FileData.Value[0, x])
                                      .ToArray();
                        // DO SAME HERE 
                        _jobData = FileData.Value;

                        if (CreateJobShop())
                        {
                            SelectAlgorithmPage();
                        }
                    }
                };

                filesFrame.Add(listView);
            }

            this.Add(titleLabel, filesFrame);
        }

        private void SelectAlgorithmPage()
        {
            this.RemoveAll();

            var algorithmsFrame = new FrameView("Available Algorithms")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(60),
                Height = Dim.Percent(60)
            };

            var titleLabel = new Label($"Select an algorithm to solve {_fileName}")
            {
                X = Pos.Center(),
                Y = Pos.Top(algorithmsFrame) - 3,
            };

            string[] algorthims = Globals.AvailableAlgorithms;
            var listView = new ListView(algorthims)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            listView.OpenSelectedItem += (args) =>
            {
                string selectedAlgorithm = args.Value.ToString();

                ErrorOr<Solver> solverResult = Globals.SolverFactory.Create(selectedAlgorithm, _jobShop);

                if (!CheckError(solverResult, 'O'))
                {
                    _selectedAlgorithm = selectedAlgorithm;
                    _solver = solverResult.Value;

                    PreRunOverviewPage();
                }
            };

            algorithmsFrame.Add(listView);
            this.Add(titleLabel, algorithmsFrame);
        }

        private void PreRunOverviewPage()
        {
            this.RemoveAll();

            var titleLabel = new Label("Overview")
            {
                X = Pos.Center(),
                Y = Pos.Top(this) + 1,
            };

            var fileLabel = new Label($"File: {_fileName}")
            {
                X = Pos.Center(),
                Y = Pos.Top(titleLabel) + 2,
            };

            var algorithmLabel = new Label($"Selected Algorithm: {_selectedAlgorithm}, {_jobData[0,0]}")
            {
                X = Pos.Center(),
                Y = Pos.Top(fileLabel) + 1,
            };

            
            var breakLine = new LineView()
            {
                Y = Pos.Top(algorithmLabel) + 2,
                Width = Dim.Fill(),
                Orientation = Orientation.Horizontal,
                Text = "Data Preview"
            };

            var previewDataTable = new TableView()
            {
                X = Pos.Center(),
                Y = Pos.Top(breakLine) + 1,
                Width = Dim.Percent(80),
                Height = Dim.Percent(50),
            };

            previewDataTable.Style.AlwaysShowHeaders = true;

            this.Add(titleLabel, fileLabel, algorithmLabel, breakLine, previewDataTable);
        }
    }
}
