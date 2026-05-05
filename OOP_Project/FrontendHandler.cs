using ErrorOr;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.Graphs;

namespace OOP_Project
{
    public class FrontendHandler : Window
    {
        // Job shop that will contain the data to be used by the solver 
        private string[] _dataHeaders;
        private string[,] _jobData;
        private JobShop _jobShop;
        // File handling variables
        private string _directoryPath = "jobs";
        private string _fullPath;
        private string _fileName;
        // Use to store alogorithm and solver
        private string _selectedAlgorithm;
        private Solver _solver;
        // Use to revert on error
        private string _lastSafeState = "file";

        // Colour Schemes
        private ColorScheme _primaryScheme;
        private ColorScheme _subtleScheme;
        private ColorScheme _frameScheme;

        public FrontendHandler()
        {
            // Set up basic window properties
            Title = "WestEng Job Shop Scheduler";
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            ColorScheme = _primaryScheme;
        }

        public void RunApp()
        {
            Application.Init();

            // Set the colour schemes
            _primaryScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Black, Color.BrightCyan),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.BrightCyan),
            };

            _subtleScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
            };

            _frameScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.BrightCyan),
            };


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
            MessageBox.ErrorQuery("Critical Error", message, "OK");
            RevertToSafe();
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
                        //Debug.Log($"Critical error occurred: {errorMessage}\n!!!\nReverting to {_lastSafeState}\n!!!\n");
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

        private void RevertToSafe()
        {
            if (_lastSafeState == "file")
            {
                JobSelectionPage();
            }
            else if (_lastSafeState == "algorithm")
            {
                SelectAlgorithmPage();
            }
            else if (_lastSafeState == "preRunOverview")
            {
                PreRunOverviewPage();
            }
            else
            {
                JobSelectionPage();
            }
        }

        // PAGE METHODS
        // -------------------------------------------------

        public void JobSelectionPage()
        {
            // New safe state
            _lastSafeState = "file";

            this.RemoveAll();

            var filesFrame = new FrameView("Available Job Files")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(60),
                Height = Dim.Percent(60),
                ColorScheme = _frameScheme
            };

            // Position the title using Pos.Top(filesFrame) and an integer offset (Pos - int is supported)
            var titleLabel = new Label("Welcome to the WestEng Job Shop Scheduler")
            {
                X = Pos.Center(),
                Y = Pos.Top(filesFrame) - 3,
                ColorScheme = _primaryScheme
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
                    Height = Dim.Fill(),
                    ColorScheme = _frameScheme
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

                        // Copies the data from FileData to _jobData, excluding the header row. Uses Array.Copy for efficiency
                        int rows = FileData.Value.GetLength(0);
                        int cols = FileData.Value.GetLength(1);
                        _jobData = new string[rows - 1, cols];
                        int elementsToCopy = (rows - 1) * cols;
                        Array.Copy(FileData.Value, cols, _jobData, 0, elementsToCopy);

                        // Creates the jobshop
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
            // New safe state
            _lastSafeState = "algorithm";

            this.RemoveAll();

            var algorithmsFrame = new FrameView("Available Algorithms")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(60),
                Height = Dim.Percent(60),
                ColorScheme = _frameScheme
            };

            var titleLabel = new Label($"Select an algorithm to solve {_fileName}")
            {
                X = Pos.Center(),
                Y = Pos.Top(algorithmsFrame) - 3,
                ColorScheme = _primaryScheme
            };

            string[] algorthims = Globals.AvailableAlgorithms;
            var listView = new ListView(algorthims)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = _frameScheme
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
            // New safe state
            _lastSafeState = "preRunOverview";

            this.RemoveAll();

            // Pre-Run Stats
            var titleLabel = new Label("Overview")
            {
                X = Pos.Center(),
                Y = Pos.Top(this) + 1,
                ColorScheme = _primaryScheme
            };

            var fileLabel = new Label($"File: {_fileName}")
            {
                X = Pos.Center(),
                Y = Pos.Top(titleLabel) + 2,
                ColorScheme = _subtleScheme
            };

            var algorithmLabel = new Label($"Selected Algorithm: {_selectedAlgorithm}")
            {
                X = Pos.Center(),
                Y = Pos.Top(fileLabel) + 1,
                ColorScheme = _subtleScheme
            };

            
            var breakLine = new LineView()
            {
                Y = Pos.Top(algorithmLabel) + 1,
                Width = Dim.Fill(),
                Orientation = Orientation.Horizontal,
                Text = "Data Preview",
                ColorScheme = _primaryScheme
            };

            // Preview Frame and Table

            ErrorOr<DataTable> errorOrDataTable = Utils.CreateDataTable(_dataHeaders, _jobData);

            if (CheckError(errorOrDataTable, 'C'))
            {
                return;
            }

            DataTable dataTable = errorOrDataTable.Value;

            var tableFrame = new FrameView("Schedule")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(breakLine) + 1,
                Width = Dim.Percent(80),
                Height = Dim.Percent(50),
                ColorScheme = _frameScheme
            };

            var previewDataTable = new TableView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = dataTable,
                CanFocus = true,
                ColorScheme = _frameScheme
            };

            previewDataTable.Style.AlwaysShowHeaders = true;

            tableFrame.Add(previewDataTable);

            // Continue Menu
            var breakLine2 = new LineView()
            {
                Y = Pos.Bottom(tableFrame) + 1,
                Width = Dim.Fill(),
                Orientation = Orientation.Horizontal,
                Text = "Data Preview",
                ColorScheme = _primaryScheme
            };

            var confirmationLabel = new Label("Do you want to continue?")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(breakLine2) + 1,
                ColorScheme = _primaryScheme
            };

            var yesButton = new Button("Yes")
            {
                X = Pos.Center() - 10,
                Y = Pos.Bottom(confirmationLabel) + 1,
                IsDefault = true,
                HotKey = Key.Enter,
                ColorScheme = _primaryScheme
            };

            var noButton = new Button("No")
            {
                X = Pos.Center() + 5,
                Y = Pos.Bottom(confirmationLabel) + 1,
                HotKey = Key.Esc,
                ColorScheme = _primaryScheme
            };

            // yes no logic
            yesButton.Clicked += () =>
            {
                // Runs the solver
                SolverPage();
            };

            noButton.Clicked += () =>
            {
                // Return to algorithm selection
                SelectAlgorithmPage();
            };

            this.Add(titleLabel, fileLabel, algorithmLabel, breakLine, tableFrame, breakLine2 ,confirmationLabel, yesButton, noButton);
        }

        private void LoadingPage()
        {
            this.RemoveAll();
            var loadingLabel = new Label("Solving, please wait...")
            {
                X = Pos.Center(),
                Y = Pos.Center()
            };
            this.Add(loadingLabel);
            Application.Refresh(); // Force UI to redraw before solver blocks the thread
        }

        private void SolverPage()
        {
            LoadingPage();

            var sw = Stopwatch.StartNew();
            ErrorOr<Schedule> EORschedule = _solver.Solve();
            var time = sw.Elapsed;
            sw.Stop();

            if (CheckError(EORschedule, 'C')) return;

            Schedule schedule = EORschedule.Value;

            // -- Stats panel --
            var titleLabel = new Label($" File: {_fileName} | Algorithm: {_selectedAlgorithm} ")
            {
                X = Pos.Center(),
                Y = 1,
                ColorScheme = new ColorScheme
                {
                    Normal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black)
                }
            };

            var statsFrame = new FrameView("Results")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(titleLabel) + 1,
                Width = 36,
                Height = 5,
                ColorScheme = new ColorScheme
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
                }
            };

            statsFrame.Add(new Label($" Makespan : {schedule.GetMakespan()}") { X = 0, Y = 0 });
            statsFrame.Add(new Label($" Solved in: {time.TotalMilliseconds:F1}ms") { X = 0, Y = 1 });


            // Excel Selection
            var excelRowY = Pos.Bottom(statsFrame) + 1;

            var excelLabel = new Label("Do you export to excel? ")
            {
                X = Pos.Center() - 21, 
                Y = excelRowY,
                ColorScheme = _primaryScheme
            };

            var yesButton = new Button("Yes")
            {
                X = Pos.Right(excelLabel) + 1,
                Y = excelRowY,
                IsDefault = true,
                HotKey = Key.Enter,
                ColorScheme = _primaryScheme
            };

            var noButton = new Button("No")
            {
                X = Pos.Right(yesButton) + 1, 
                Y = excelRowY,
                HotKey = Key.Esc,
                ColorScheme = _primaryScheme
            };

            // yes no logic
            yesButton.Clicked += () =>
            {
                // Runs the solver
                ExcelHandler.ExportScheduleToExcel(schedule, "schedules");
            };

            noButton.Clicked += () =>
            {
                // Return to algorithm selection
                SelectAlgorithmPage();
            };

            var breakLine = new LineView()
            {
                Y = Pos.Bottom(yesButton) + 1,
                Width = Dim.Fill(),
                Orientation = Orientation.Horizontal,
            };

            // -- Schedule table --
            ErrorOr<DataTable> errorOrDataTable = Utils.CreateDataTable(
                schedule.GetHeaders(), schedule.GetSchedule(), hori: true);

            if (CheckError(errorOrDataTable, 'C')) return;

            var tableFrame = new FrameView("Schedule")
            {
                X = 1,
                Y = Pos.Bottom(breakLine) + 1,
                Width = Dim.Fill(1),
                Height = Dim.Fill(1),
                ColorScheme = _frameScheme
            };

            var scheduleTable = new TableView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = errorOrDataTable.Value,
                CanFocus = true,
                Style = new TableView.TableStyle
                {
                    AlwaysShowHeaders = true,
                    ExpandLastColumn = true,
                },
                // move your ColorScheme here unchanged
            };

            tableFrame.Add(scheduleTable);


            this.RemoveAll();
            this.Add(titleLabel, statsFrame, excelLabel,yesButton,noButton,breakLine, tableFrame);
        }
    }
}
