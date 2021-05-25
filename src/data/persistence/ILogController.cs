using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Meminisse
{
    public interface ILogController
    {
        int logBufferCount { get; }

        void Init(string filename, List<LogEntity> entitiesToLog);

        void InitWithHeader(string filename, List<LogEntity> entitiesToLog, LogHeader logHeader);

        void Reset();

        void Add(long totalElapsedTimeMs, List<ILogEntity> entities);

        void FlushToFile();
    }
}