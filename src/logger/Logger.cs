using System;
using System.Threading;

/// <summary>
/// A thread safe logging class
/// </summary>
public class Logger
{
    private Mutex mut = new Mutex(initiallyOwned: false);

    private enum Level { TRACE, DEBUG, INFO, WARNING, ERROR };

    /// <summary>
    /// Write Trace log
    /// </summary>
    /// <param name="message"></param>
    public void T(string message)
    {
        Log(message, Level.TRACE);
    }

    /// <summary>
    /// Write Debug log
    /// </summary>
    /// <param name="message"></param>
    public void D(string message)
    {
        Log(message, Level.DEBUG);
    }

    /// <summary>
    /// Write Info log
    /// </summary>
    /// <param name="message"></param>
    public void I(string message)
    {
        Log(message, Level.INFO);
    }

    /// <summary>
    /// Write Warning log
    /// </summary>
    /// <param name="message"></param>
    public void W(string message)
    {
        Log(message, Level.WARNING);
    }

    /// <summary>
    /// Write Error log
    /// </summary>
    /// <param name="message"></param>
    public void E(string message)
    {
        Log(message, Level.ERROR);
    }
    private void Log(string message, Level level)
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