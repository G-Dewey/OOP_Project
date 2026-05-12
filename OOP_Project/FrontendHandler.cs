using DocumentFormat.OpenXml.Office2016.Drawing.Command;
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
    /// <summary>
    /// Handles the entire Terminal User Interface (TUI) for the application.
    /// Inherits from Terminal.Gui.Window to act as the main application container.
    /// </summary>
    public class FrontendHandler : Window
    {
        // --- State Management ---
        // These variables track where the user is in the application to control UI rendering.
        private bool _isRunning = true;
        private string _currentPage;
        private string _lastPage;

        // --- Data & Domain Models ---
        // Holds the raw data and the parsed domain model needed by the solver algorithms.
        private string[] _dataHeaders;
        private string[,] _jobData;
        private JobShop _jobShop; // The core business logic object representing the jobs to be scheduled

        // --- File & Solver Settings ---
        // Manages paths for importing raw data and exporting results.
        private string _importDirectory = "jobs";
        private string _fileName;
        private string _fullPath;
        private string _exportDirectory = "schedules";
        private string _selectedAlgorithm;
        private Solver _solver; // The active algorithm instance selected by the user

        // Keeps track of the last stable UI page to return to if a critical error occurs.
        private string _lastSafeState = "file";

        // --- UI Styling (Color Schemes) ---
        // Terminal.Gui uses ColorSchemes to define how elements look in different states (normal, focused, etc.)
        private ColorScheme _primaryScheme;
        private ColorScheme _subtleScheme;
        private ColorScheme _frameScheme;

        /// <summary>
        /// Constructor: Sets up the main window dimensions and default styling.
        /// </summary>
        public FrontendHandler()
        {
            Title = "WestEng Job Shop Scheduler";
            X = 0; // Start at the absolute left edge of the terminal
            Y = 0; // Start at the absolute top edge of the terminal

            // Dim.Fill() tells the UI component to take up all remaining space in its parent container.
            Width = Dim.Fill();
            Height = Dim.Fill();
            ColorScheme = _primaryScheme;
        }

        /// <summary>
        /// Bootstraps the Terminal.Gui application, initializes styles, and starts the UI loop.
        /// </summary>
        public void RunApp()
        {
            // Must be called before creating or using any Terminal.Gui elements.
            Application.Init();

            // --- Define Color Schemes ---
            // MakeAttribute combines a foreground and background color.

            // Primary scheme used for standard text and buttons
            _primaryScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.Black, Color.BrightCyan), // Inverts when highlighted
                HotNormal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.BrightCyan),
            };

            // Subtle scheme used for secondary text/labels so they don't distract the user
            _subtleScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
            };

            // Frame scheme used specifically for container borders and list views
            _frameScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                HotNormal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.BrightCyan),
            };

            // Add this specific Window instance to the application's root (Top) container
            Application.Top.Add(this);

            // Load the initial view
            JobSelectionPage();

            _isRunning = true;
            _currentPage = "Job Selection";

            // --- State-Driven UI Loop ---
            // Instead of manually calling draw methods, we check every 100ms if the page state has changed.
            // If the user triggered an action that changes `_currentPage`, we clear the screen and load the new page.
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), (MainLoop loop) =>
            {
                if (_currentPage != _lastPage)
                {
                    this.RemoveAll(); // Clear all current child views from the window

                    // Route to the appropriate rendering method based on the current state string
                    switch (_currentPage)
                    {
                        case "Algorithm Selection": SelectAlgorithmPage(); break;
                        case "Pre-Run Overview": PreRunOverviewPage(); break;
                        case "Solver": SolverPage(); break;
                        case "Job Selection": JobSelectionPage(); break;
                    }

                    _lastPage = _currentPage; // Sync the state tracker
                    Application.Refresh();    // Force the terminal to redraw the new elements
                }
                return true; // Returning true keeps the timeout looping indefinitely
            });

            // Start the application blocking loop. Code below this line won't run until the app shuts down.
            Application.Run();
            Application.Shutdown();
        }

        // --- ERROR HANDLING METHODS ---
        // These methods standardize how the UI responds to different severities of exceptions or failed validations.

        /// <summary>
        /// Handles server/system issues. Alerts the user and returns them to the last known safe menu.
        /// </summary>
        private void CriticalError(string message)
        {
            MessageBox.ErrorQuery("Critical Error", message, "OK");
            RevertToSafe();
        }

        /// <summary>
        /// Handles user-driven data errors (e.g., bad file format). 
        /// Alerts the user but leaves them on the current screen to fix the issue.
        /// </summary>
        private void SelectionError(string message)
        {
            MessageBox.ErrorQuery("Error", message, "OK");
        }

        /// <summary>
        /// Displays a non-blocking alert for issues that don't prevent the algorithm from running.
        /// </summary>
        private void Warning(string message)
        {
            MessageBox.Query("Warning", message, "OK");
        }

        /// <summary>
        /// Evaluates an IErrorOr response object. If it contains an error, it extracts the message 
        /// and triggers the appropriate UI alert based on the passed severity character (C, W, S, O).
        /// Returns true if an error was found and handled.
        /// </summary>
        private bool CheckError(IErrorOr item, char severity)
        {
            if (item.IsError)
            {
                // Combine all error descriptions into a single readable string
                var errorMessage = string.Join(", ", item.Errors.Select(e => e.Description));

                switch (severity)
                {
                    case 'C': CriticalError(errorMessage); return true;
                    case 'W': Warning(errorMessage); return true;
                    case 'S': SelectionError(errorMessage); return true;
                    case 'O': return true; // 'O' for Other: silently return true without a popup
                    default: return true;
                }
            }
            return false;
        }

        // --- UTIL METHODS ---

        /// <summary>
        /// Reads the selected data file using the appropriate parser based on the file extension.
        /// Uses the Factory pattern concept, currently supporting only CSV.
        /// </summary>
        private ErrorOr<string[,]> ReadJobFile(string fileType)
        {
            IFileReader reader;

            if (fileType == "csv")
            {
                reader = new CSVReader();
            }
            else
            {
                return Error.Unexpected(description: "Unsupported file type");
            }

            return reader.ReadFile(_fullPath);
        }

        /// <summary>
        /// Attempts to convert the parsed raw 2D array into the application's core JobShop domain object.
        /// </summary>
        public bool CreateJobShop()
        {
            ErrorOr<JobShop> jobShopResult = JobShop.Create(_jobData);

            if (CheckError(jobShopResult, 'C'))
            {
                return false; // Stop execution if creation failed
            }

            _jobShop = jobShopResult.Value;
            return true;
        }

        /// <summary>
        /// Routing mechanism used specifically for recovering from Critical errors.
        /// </summary>
        private void RevertToSafe()
        {
            if (_lastSafeState == "file") { _currentPage = "Job Selection"; }
            else if (_lastSafeState == "algorithm") { _currentPage = "Algorithm Selection"; }
            else if (_lastSafeState == "preRunOverview") { _currentPage = "Pre-Run Overview"; }
            else { _currentPage = "Job Selection"; } // Default fallback
        }

        /// <summary>
        /// Opens a Terminal-native directory selector so the user can pick import/export locations.
        /// </summary>
        private string SelectFolder()
        {
            var folderDialog = new OpenDialog("Select Export Folder", "Choose a folder to find job files in")
            {
                CanChooseFiles = false,       // We only want folders, not specific files
                CanChooseDirectories = true
            };

            Application.Run(folderDialog); // Blocks until the dialog is closed

            if (!folderDialog.Canceled)
            {
                return folderDialog.FilePath.ToString();
            }

            // Cleanup memory/handlers if the user cancelled
            folderDialog.Dispose();
            return null;
        }

        // --- PAGE DEFINITIONS ---
        // Each method below acts as a "View" builder. They construct the UI elements, 
        // position them, bind event handlers, and add them to the main Window.

        // PAGE 1: FILE SELECTION
        public void JobSelectionPage()
        {
            _lastSafeState = "file";
            this.RemoveAll(); // Ensure a clean slate

            string selectedExtension = "csv";
            string[] supportedExtensions = ["csv", "txt", "json"];

            // A FrameView creates a visible box with a title to group related elements
            var filesFrame = new FrameView("Available Job Files")
            {
                X = Pos.Center(),         // Horizontally center the frame
                Y = Pos.Center(),         // Vertically center the frame
                Width = Dim.Percent(60),  // Take up 60% of the terminal width
                Height = Dim.Percent(60), // Take up 60% of the terminal height
                ColorScheme = _frameScheme
            };

            var titleLabel = new Label("Welcome to the WestEng Job Shop Scheduler")
            {
                X = Pos.Center(),
                Y = Pos.Top(filesFrame) - 4, // Position it 4 rows above the top edge of the frame
                ColorScheme = _primaryScheme
            };

            // --- Top Toolbar inside the Frame ---
            var folderLabel = new Label($"Folder: {Utils.TruncatePath(_importDirectory, 30)}")
            {
                X = 0,
                Y = 0,
                Width = 40,
                ColorScheme = _primaryScheme
            };

            // Pos.Right() anchors this button immediately to the right of the folderLabel
            var folderButton = new Button("Browse...")
            {
                X = Pos.Right(folderLabel) + 1,
                Y = 0,
                ColorScheme = _primaryScheme
            };

            var extLabel = new Label("Type:")
            {
                X = Pos.Right(folderButton) + 2,
                Y = 0,
                ColorScheme = _primaryScheme
            };

            var extRadio = new RadioGroup(supportedExtensions.Select(e => (NStack.ustring)e).ToArray())
            {
                X = Pos.Right(extLabel) + 1,
                Y = 0,
                ColorScheme = _primaryScheme,
                SelectedItem = Array.IndexOf(supportedExtensions, selectedExtension) // Pre-select current type
            };

            var separator = new LineView(Orientation.Horizontal)
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill()
            };

            // Add all toolbar elements to the frame
            filesFrame.Add(folderLabel, folderButton, extLabel, extRadio, separator);

            // Container for the list of files, positioned below the toolbar (Y = 2)
            var listContainer = new View()
            {
                X = 0,
                Y = 2,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // --- Event Handlers ---

            // When 'Browse' is clicked, open the folder dialog and refresh the list if a new folder is chosen
            folderButton.Clicked += () =>
            {
                string? chosen = SelectFolder();
                if (!string.IsNullOrWhiteSpace(chosen))
                {
                    _importDirectory = chosen;
                    folderLabel.Text = $"Folder: {Utils.TruncatePath(_importDirectory, 30)}";
                    RefreshFileList();
                }
            };

            // When a different file type is selected, refresh the list to show only those files
            extRadio.SelectedItemChanged += (args) =>
            {
                selectedExtension = supportedExtensions[args.SelectedItem];
                RefreshFileList();
            };

            filesFrame.Add(listContainer);
            this.Add(titleLabel, filesFrame); // Add the main elements to the Window

            // Load the initial list of files
            RefreshFileList();

            // Local helper method to rebuild the file list whenever the directory or extension changes
            void RefreshFileList()
            {
                listContainer.RemoveAll(); // Clear previous list

                ErrorOr<string[]> fileResult = Utils.GetFilesInDir(_importDirectory, selectedExtension);

                if (CheckError(fileResult, 'O'))
                {
                    // If reading the directory failed, show the error text instead of the list
                    var errorText = string.Join(Environment.NewLine, fileResult.Errors.Select(e => e.Description));
                    listContainer.Add(new Label(errorText) { X = 0, Y = 0, ColorScheme = Colors.Error });
                }
                else
                {
                    // Clean up file paths so only the file name is shown in the UI
                    string[] cleanedFileName = new string[fileResult.Value.Length];
                    for (int i = 0; i < fileResult.Value.Length; i++)
                    {
                        cleanedFileName[i] = Utils.RemovePath(fileResult.Value[i], _importDirectory);
                    }

                    // Create the interactive list view
                    var listView = new ListView(cleanedFileName)
                    {
                        X = 0,
                        Y = 0,
                        Width = Dim.Fill(),
                        Height = Dim.Fill(),
                        ColorScheme = _frameScheme
                    };

                    // Action triggered when the user presses Enter on a specific file in the list
                    listView.OpenSelectedItem += (args) =>
                    {
                        _fileName = args.Value.ToString();
                        _fullPath = System.IO.Path.GetFullPath($"{_importDirectory}\\{_fileName}");

                        ErrorOr<string[,]> fileData = ReadJobFile(selectedExtension);

                        if (!CheckError(fileData, 'C')) // Only proceed if no critical errors during read
                        {
                            int rows = fileData.Value.GetLength(0);
                            int cols = fileData.Value.GetLength(1);

                            // Extract the first row as headers
                            _dataHeaders = Enumerable.Range(0, cols)
                                .Select(x => fileData.Value[0, x])
                                .ToArray();

                            // Extract everything else as raw data
                            _jobData = new string[rows - 1, cols];
                            Array.Copy(fileData.Value, cols, _jobData, 0, (rows - 1) * cols);

                            // Attempt to build the domain model. If successful, move to next page.
                            if (CreateJobShop())
                                _currentPage = "Algorithm Selection";
                        }
                    };

                    listContainer.Add(listView);
                }

                listContainer.SetNeedsDisplay(); // Force the container to redraw
            }
        }

        // PAGE 2: SELECT ALGORITHM PAGE
        private void SelectAlgorithmPage()
        {
            _lastSafeState = "algorithm";
            this.RemoveAll();

            // Create a universal back button
            var backButton = new Button("Back")
            {
                X = 1,
                Y = 1,
                IsDefault = true,
                HotKey = Key.Esc, // Allows user to hit 'Escape' to trigger this button
                ColorScheme = _primaryScheme
            };

            backButton.Clicked += () =>
            {
                _currentPage = "Job Selection";
                return;
            };

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
                Y = Pos.Top(algorithmsFrame) - 2,
                ColorScheme = _primaryScheme
            };

            // Grab the available algorithms from a global store
            string[] algorithms = Globals.AvailableAlgorithms;
            var listView = new ListView(algorithms)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = _frameScheme
            };

            // When an algorithm is selected...
            listView.OpenSelectedItem += (args) =>
            {
                string selectedAlgorithm = args.Value.ToString();

                // Use the Factory pattern to create the correct solver based on the string name
                ErrorOr<Solver> solverResult = Globals.SolverFactory.Create(selectedAlgorithm, _jobShop);

                if (!CheckError(solverResult, 'O'))
                {
                    _selectedAlgorithm = selectedAlgorithm;
                    _solver = solverResult.Value;

                    // Transition state to overview page
                    _currentPage = "Pre-Run Overview";
                    return;
                }
            };

            algorithmsFrame.Add(listView);
            this.Add(backButton, titleLabel, algorithmsFrame);
        }

        // PAGE 3: PRE-RUN OVERVIEW
        private void PreRunOverviewPage()
        {
            _lastSafeState = "preRunOverview";
            this.RemoveAll();

            var backButton = new Button("Back")
            {
                X = 1,
                Y = 1,
                IsDefault = true,
                HotKey = Key.Esc,
                ColorScheme = _primaryScheme
            };

            backButton.Clicked += () =>
            {
                _currentPage = "Algorithm Selection";
                return;
            };

            // --- Pre-Run Stat Layout ---
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

            // Horizontal line to divide sections visually
            var breakLine = new LineView()
            {
                Y = Pos.Top(algorithmLabel) + 1,
                Width = Dim.Fill(),
                Orientation = Orientation.Horizontal,
                Text = "Data Preview",
                ColorScheme = _primaryScheme
            };

            // --- Data Table Preview ---
            // Convert our raw 2D array data into a C# DataTable, which Terminal.Gui knows how to render
            ErrorOr<DataTable> errorOrDataTable = Utils.CreateDataTable(_dataHeaders, _jobData);

            if (CheckError(errorOrDataTable, 'C'))
            {
                return;
            }

            DataTable dataTable = errorOrDataTable.Value;

            var tableFrame = new FrameView("Jobs")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(breakLine) + 1,
                Width = Dim.Percent(80),
                Height = Dim.Percent(50),
                ColorScheme = _frameScheme
            };

            // Initialize the Terminal.Gui TableView to display the DataTable
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

            // --- Confirmation Menu ---
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

            // Pos.Center() - 10 pushes the button slightly left of center
            var yesButton = new Button("Yes")
            {
                X = Pos.Center() - 10,
                Y = Pos.Bottom(confirmationLabel) + 1,
                IsDefault = true,
                HotKey = Key.Enter,
                ColorScheme = _primaryScheme
            };

            // Pos.Center() + 5 pushes the button slightly right of center
            var noButton = new Button("No")
            {
                X = Pos.Center() + 5,
                Y = Pos.Bottom(confirmationLabel) + 1,
                HotKey = Key.Esc,
                ColorScheme = _primaryScheme
            };

            yesButton.Clicked += () =>
            {
                _currentPage = "Solver"; // Proceed to actual computation
                return;
            };

            noButton.Clicked += () =>
            {
                _currentPage = "Algorithm Selection"; // Go back if user changed their mind
                return;
            };

            this.Add(backButton, titleLabel, fileLabel, algorithmLabel, breakLine, tableFrame, breakLine2, confirmationLabel, yesButton, noButton);
        }

        /// <summary>
        /// Displays a brief, non-interactive "Loading" view. Because the solver blocks the main thread,
        /// we must force the application to draw this screen BEFORE executing the solver logic.
        /// </summary>
        private void LoadingPage()
        {
            this.RemoveAll();
            var loadingLabel = new Label("Solving, please wait...")
            {
                X = Pos.Center(),
                Y = Pos.Center()
            };
            this.Add(loadingLabel);
            Application.Refresh(); // Force UI to redraw immediately
        }

        // PAGE 4: SOLVER EXEUCTION AND RESULTS
        private void SolverPage()
        {
            // 1. Show the loading screen
            LoadingPage();

            var backButton = new Button("Back")
            {
                X = 1,
                Y = 1,
                IsDefault = true,
                HotKey = Key.Esc,
                ColorScheme = _primaryScheme
            };

            backButton.Clicked += () =>
            {
                _currentPage = "Pre-Run Overview";
                return;
            };

            // 2. Execute the heavy lifting (Solver logic) while tracking how long it takes
            var sw = Stopwatch.StartNew();
            ErrorOr<Schedule> EORschedule = _solver.Solve(); // Blocks thread until complete
            var time = sw.Elapsed;
            sw.Stop();

            // If the algorithm crashed, abort drawing the results
            if (CheckError(EORschedule, 'C')) return;

            Schedule schedule = EORschedule.Value;

            // --- Stats Panel ---
            // Shows metadata about the solved job
            var titleLabel = new Label($" File: {_fileName} | Algorithm: {_selectedAlgorithm} ")
            {
                X = Pos.Center(),
                Y = 1,
                // Creating a custom inline color scheme just for this header to make it pop
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

            // Retrieve performance metrics from the completed schedule object
            statsFrame.Add(new Label($" Makespan : {schedule.GetMakespan()}") { X = 0, Y = 0 });
            statsFrame.Add(new Label($" Solved in: {time.TotalMilliseconds:F1}ms") { X = 0, Y = 1 });


            // --- Excel Export Selection Menu ---
            var excelRowY = Pos.Bottom(statsFrame) + 1; // Calculate Y-coord once to keep elements horizontally aligned

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

            // Logic to export the output to a physical excel file
            yesButton.Clicked += () =>
            {
                _exportDirectory = SelectFolder();
                if (_exportDirectory != null) // Ensure user didn't cancel the folder prompt
                {
                    ExcelHandler.ExportScheduleToExcel(schedule, _exportDirectory);
                    MessageBox.Query("Success", "Solution has been saved successfully.", "Ok");
                }
            };

            noButton.Clicked += () =>
            {
                _currentPage = "Pre-Run Overview"; // Send them back to the start of this flow
                return;
            };

            var breakLine = new LineView()
            {
                Y = Pos.Bottom(yesButton) + 1,
                Width = Dim.Fill(),
                Orientation = Orientation.Horizontal,
            };

            // --- Render the Final Schedule Table ---
            // Converts the schedule output into a horizontal DataTable format
            ErrorOr<DataTable> errorOrDataTable = Utils.CreateDataTable(
                schedule.GetHeaders(), schedule.GetSchedule(), hori: true);

            if (CheckError(errorOrDataTable, 'C')) return;

            var tableFrame = new FrameView("Schedule")
            {
                X = 1,
                Y = Pos.Bottom(breakLine) + 1,
                Width = Dim.Fill(1), // Fill remaining width minus 1 character margin
                Height = Dim.Fill(1), // Fill remaining height minus 1 character margin
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
                    ExpandLastColumn = true, // Prevents weird empty space if data is narrow
                },
            };

            tableFrame.Add(scheduleTable);

            // Clear the "Loading" text and add all the calculated result UI elements
            this.RemoveAll();
            this.Add(backButton, titleLabel, statsFrame, excelLabel, yesButton, noButton, breakLine, tableFrame);

            // Attach a global key listener for this specific page
            // Pressing F2 allows the user to re-run the exact same solver without navigating menus
            this.KeyPress += (KeyEventEventArgs e) =>
            {
                if (e.KeyEvent.Key == Key.F2)
                {
                    _currentPage = "Solver";
                    _lastPage = ""; // Resetting last page forces the UI MainLoop to trigger a redraw/reload
                    e.Handled = true; // Tell the system the key press was processed

                    return;
                }
            };
        }
    }
}