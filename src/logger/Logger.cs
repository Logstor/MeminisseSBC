using System;
using System.Threading;

/// <summary>
/// 
/// </summary>
public enum LogLevel { TRACE = 0, DEBUG = 1, INFO = 2, WARNING = 3, ERROR = 4 };

/// <summary>
/// A thread safe logging class
/// </summary>
public class Logger
{
    private Mutex mut = new Mutex(initiallyOwned: false);

    private LogLevel logLevel { get; set; }

    public Logger(LogLevel logLevel = LogLevel.WARNING)
    {
        this.logLevel = logLevel;
    }

    /// <summary>
    /// Write Trace log
    /// </summary>
    /// <param name="message"></param>
    public void T(string message)
    {
        if (this.logLevel >= LogLevel.TRACE)
            Log(message, LogLevel.TRACE);
    }

    /// <summary>
    /// Write Debug log
    /// </summary>
    /// <param name="message"></param>
    public void D(string message)
    {
        if (this.logLevel >= LogLevel.DEBUG)
            Log(message, LogLevel.DEBUG);
    }

    /// <summary>
    /// Write Info log
    /// </summary>
    /// <param name="message"></param>
    public void I(string message)
    {
        if (this.logLevel >= LogLevel.INFO)
            Log(message, LogLevel.INFO);
    }

    /// <summary>
    /// Write Warning log
    /// </summary>
    /// <param name="message"></param>
    public void W(string message)
    {
        if (this.logLevel >= LogLevel.WARNING)
            Log(message, LogLevel.WARNING);
    }

    /// <summary>
    /// Write Error log
    /// </summary>
    /// <param name="message"></param>
    public void E(string message)
    {
        if (this.logLevel >= LogLevel.ERROR)
            Log(message, LogLevel.ERROR);
    }

    private void Log(string message, LogLevel level)
    {
        // Retrieve lock
        mut.WaitOne();

        // Write the log
        string output = string.Format("{0}: {1}", level.ToString(), message);
        Console.WriteLine(output);

        // Release Mutex
        mut.ReleaseMutex();
    }
}