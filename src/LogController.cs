using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;

using DuetAPIClient;
using DuetAPI;
using DuetAPI.Connection;
using DuetAPI.ObjectModel;

/// <summary>
/// Log controller that is responsible for logging a single job
/// </summary>
public class LogController
{
    private IDataAccess dataAccess = CodeDataAccess.getInstance();

    private Logger logger;

    /// <summary>
    /// Creates an instance of the logging controller.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="initialState"></param>
    public LogController(Logger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Starts the logging loop.
    /// </summary>
    /// <param name="updateFreq">How many updates per second</param>
    public async Task start(int updateFreq)
    {
        // Calculate delay
        long delayms = 1000L / (long) updateFreq;

        Stopwatch timer = new();
        MachineStatus status;
        PositionEntity positionEntity;

        // Loop until Job is done or the machine is turned off
        do {
            timer.Restart();

            status = await this.dataAccess.requestStatus();

            // If prints is paused
            if (status == MachineStatus.Paused || status == MachineStatus.Pausing)
            {
                this.logger.T("Machine Paused");
                Thread.Sleep((int) delayms);
                continue;
            }

            // If print is done or halted then break
            else if (status == MachineStatus.Idle || status == MachineStatus.Off || status == MachineStatus.Halted)
            {
                this.logger.T("Machine Idle");
                break;
            }

            // Write to log
            positionEntity = await this.dataAccess.requestPosition();
            this.logger.T(string.Format("Position [{0}, {1}, {2}]", positionEntity.getX(), positionEntity.getY(), positionEntity.getZ()));

            timer.Stop();
            this.logger.T(string.Format("Log took {0} milliseconds", timer.ElapsedMilliseconds));
            
            // Wait if we're before time
            if ( !(timer.ElapsedMilliseconds >= delayms) )
                Thread.Sleep( (int) (delayms - timer.ElapsedMilliseconds) );

            // Otherwise warn the we can't reach the desired frequency
            else
                this.logger.W(
                    string.Format("We can't log this fast! Current log time: {0} ms - Max log time: {1} ms", 
                        timer.ElapsedMilliseconds, delayms));
        } 
        while (true);
    }
}