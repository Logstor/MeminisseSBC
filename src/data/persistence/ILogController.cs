using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Meminisse
{
    public interface ILogController
    {
        void Init(string filename, List<LogEntity> entitiesToLog);

        void Reset();

        void Add(long totalElapsedTimeMs, List<ILogEntity> entities);

        void FlushToFile();
    }
}