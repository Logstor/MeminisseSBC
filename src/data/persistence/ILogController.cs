using System.Threading.Tasks;

namespace Meminisse
{
    public interface ILogController<Entity>
    {
        void Init(string filename);

        void Reset();

        /// <summary>
        /// Adds a log entry.
        /// 
        /// OBS: Make sure to FlushToFile() to persist the logs and make sure the buffer don't get to big.
        /// </summary>
        /// <param name="elapsedTimeMs">Total Elapsed time in ms</param>
        /// <param name="entity">The entity to log</param>
        void Add(long elapsedTimeMs, Entity entity);

        void FlushToFile();
    }
}