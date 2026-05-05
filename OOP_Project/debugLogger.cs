//using System;
//using System.Diagnostics;
//using System.IO;
//using System.IO.Pipes;

//public class DebugLogger : IDisposable
//{
//    private NamedPipeServerStream _pipeServer;
//    private StreamWriter _writer;
//    private Process _terminalProcess;
//    private readonly string _pipeName;

//    public DebugLogger()
//    {
//        // Generate a unique pipe name so multiple app instances don't collide
//        _pipeName = "DebugPipe_" + Guid.NewGuid().ToString("N");
//    }

//    /// <summary>
//    /// Opens the new PowerShell terminal window and establishes the connection.
//    /// </summary>
//    public void OpenTerminal()
//    {
//        if (_terminalProcess != null && !_terminalProcess.HasExited)
//            return;

//        // 1. Create the Named Pipe Server
//        _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.Out, 1);

//        // 2. The PowerShell script that acts as our logging client
//        string psCommand =
//            $"$pipe = New-Object System.IO.Pipes.NamedPipeClientStream('.', '{_pipeName}', [System.IO.Pipes.PipeDirection]::In);" +
//            "$pipe.Connect();" +
//            "$reader = New-Object System.IO.StreamReader($pipe);" +
//            "while($true) { $line = $reader.ReadLine(); if ($null -eq $line) { break } Write-Host $line };";

//        // 3. Launch PowerShell in a new window
//        var startInfo = new ProcessStartInfo
//        {
//            FileName = "powershell.exe",
//            Arguments = $"-NoProfile -Command \"{psCommand}\"",
//            UseShellExecute = true // This is critical: it forces a new OS window to open
//        };

//        _terminalProcess = Process.Start(startInfo);

//        // 4. Wait for the new PowerShell window to connect to our pipe
//        _pipeServer.WaitForConnection();

//        // 5. Set up the writer
//        _writer = new StreamWriter(_pipeServer) { AutoFlush = true };
//        Log("External debugger connected.");
//    }

//    /// <summary>
//    /// Logs a message over the pipe to the external terminal.
//    /// </summary>
//    public void Log(string message)
//    {
//        // Auto-open if not connected
//        if (_writer == null)
//        {
//            OpenTerminal();
//        }

//        try
//        {
//            _writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
//        }
//        catch (IOException)
//        {
//            // If we catch an IOException, the user likely clicked the red "X" 
//            // on the external terminal. Clean up so the next Log() call opens a new one.
//            Dispose();
//        }
//    }

//    /// <summary>
//    /// Cleans up resources and closes the external terminal automatically.
//    /// </summary>
//    public void Dispose()
//    {
//        _writer?.Dispose();
//        _pipeServer?.Dispose();
//        _writer = null;
//        _pipeServer = null;
//    }
//}

//public static class Debug
//{
//    // The underlying instance is private to this class
//    private static readonly DebugLogger _instance = new DebugLogger();

//    // The public method accessible from any file
//    public static void Log(string message)
//    {
//        _instance.Log(message);
//    }

//    // Call this when the app closes to clean up the pipe/process
//    public static void ShutDown()
//    {
//        _instance.Dispose();
//    }
//}