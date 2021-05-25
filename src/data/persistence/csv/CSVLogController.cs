using System;
using System.IO;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Newtonsoft.Json;

using CsvHelper;
using CsvHelper.Configuration;

namespace Meminisse
{
    public class CSVLogController : ILogController, IDisposable
    {
        private bool initialized = false;

        private MemoryStream cache;

        private StreamWriter writer;

        private CsvWriter csv;

        private string pathToFile;

        private CsvConfiguration config;

        private int _logBufferCount { get; set; }

        /// <summary>
        /// Reflects how many log entries we currently have in the buffer.
        /// </summary>
        /// <value></value>
        int ILogController.logBufferCount 
        {
            get { return this._logBufferCount; }
        }

        public CSVLogController()
        {
            this.config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Config.instance.CSVDelimiter
            };
            this.CreateStreams();
        }

        void IDisposable.Dispose()
        {
            this.Clean();
        }

        /// <summary>
        /// Initialize the log with File Creation and Writing the initial header.
        /// 
        /// OBS: ATM you need to preserve the order of the list, so log entries can be added correctly later.
        /// </summary>
        /// <param name="filename">The name of the logfile with extension. The function will take care of the path.</param>
        /// <param name="entityTypes"></param>
        void ILogController.Init(string filename, List<LogEntity> entityTypes)
        {
            this.Initialize(filename, entityTypes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename">The name of the logfile with extension. The function will take care of the path.</param>
        /// <param name="entitiesToLog"></param>
        /// <param name="logHeader"></param>
        void ILogController.InitWithHeader(string filename, List<LogEntity> entityTypes, LogHeader logHeader)
        {
            // Write LogHeader to file
            string jsonHeader = JsonConvert.SerializeObject(logHeader, Formatting.Indented);
            writer.Write(jsonHeader);
            writer.Write("\n\n");

            // Then run normal init sequence
            this.Initialize(filename, entityTypes);
        }

        void ILogController.Reset()
        {
            // Clear and Create new instances
            this.pathToFile = null;
            this.Clean();
            this.CreateStreams();

            // Set initialized
            this.initialized = false;
        }

        /// <summary>
        /// Adding an entry to the CSV.
        /// 
        /// OBS: Important that the order of the objects in the List is the same as when 
        /// the log got initialized!
        /// </summary>
        /// <param name="totalElapsedTimeMs"></param>
        /// <param name="entities"></param>
        void ILogController.Add(long totalElapsedTimeMs, List<ILogEntity> entities)
        {
            if (!this.initialized)
                throw new Exception("LogController not initialized!");

            // Increment log buffer count
            ++this._logBufferCount;

            // Write to cache
            this.csv.WriteField(string.Format("{0}", totalElapsedTimeMs));
            foreach(ILogEntity entity in entities)
            {
                // Write the correct header
                switch(entity.GetEntityType())
                {
                    case LogEntity.Position:
                        this.csv.WriteRecord<Position>((Position) entity);
                        break;
                    case LogEntity.Time:
                        this.csv.WriteRecord<Time>((Time) entity);
                        break;
                    case LogEntity.Speed:
                        this.csv.WriteRecord<Speed>((Speed) entity);
                        break;
                    case LogEntity.Extrusion:
                        this.csv.WriteRecord<Extrusion>((Extrusion) entity);
                        break;
                    case LogEntity.Babystep:
                        this.csv.WriteRecord<Babystep>((Babystep) entity);
                        break;
                    case LogEntity.Voltage:
                        this.csv.WriteRecord<Voltage>((Voltage) entity);
                        break;
                    default:
                        throw new Exception(string.Format("LogEntity not recognized: {0}", entity.ToString()));
                }
            }

            // Goto next line
            this.csv.NextRecord();

            // Flush everything to MemoryStream
            this.csv.Flush();
        }

        void ILogController.FlushToFile()
        {
            // Make sure things is initialized
            if (!this.initialized)
                throw new Exception("LogController not initialized!");

            // Check there's somthing in the buffer
            if (this.cache.Length <= 0) 
            {
                Logger.instance.D("Calling FlushToFile() with empty log buffer");
                return;
            }

            try 
            {
                // Write to file
                using (FileStream fs = File.Open(this.pathToFile, FileMode.Append))
                {
                    fs.Write(this.cache.GetBuffer(), 0, (int) this.cache.Length);
                }

                // Reset cache and logBufferCount
                this.ResetCache();
                this._logBufferCount = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Initializing the csv format to a file by putting in all the headers. 
        /// 
        /// This is appending to the file, so it won't delete or overwrite the given file.
        /// 
        /// OBS: The list of entities to log has to be in the same order later, when log entries is being written.
        /// </summary>
        /// <param name="filename">The name of the logfile with extension. The function will take care of the path.</param>
        /// <param name="entityTypes">List of types to log.</param>
        private void Initialize(string filename, List<LogEntity> entityTypes)
        {
            this.csv.WriteField("totTime"); // Manuel header element
            
            // Set Context and WriteHeaders
            foreach (LogEntity entity in entityTypes)
            {
                (ILogEntity logEntity, ClassMap map) = this.GetClassAndMap(entity);
                this.csv.Context.RegisterClassMap(map);
                this.csv.WriteHeader(logEntity.GetType());
            }
            this.csv.NextRecord();

            // Create file
            this.CreatePathAndFile(filename);

            // Set initialized
            this.initialized = true;
        }

        /// <summary>
        /// Resetting the cache, MemoryStream, by setting the position and length to 0.
        /// 
        /// This makes the MemoryStream start overwriting the memory from the start.
        /// </summary>
        private void ResetCache()
        {
            this.cache.Position = 0;
            this.cache.SetLength(0);
        }

        private void Clean()
        {
            if (this.csv != null)
            {
                try { this.csv.Dispose(); }
                catch (Exception) {}
            }
            if (this.writer != null)
            {
                try { this.writer.Dispose(); }
                catch (Exception) {}
            }
            if (this.cache != null)
            {
                try { this.cache.Dispose(); }
                catch (Exception) {}
            } 
        }
        
        private void CreateStreams()
        {
            this.cache = new MemoryStream(128);
            this.writer = new StreamWriter(this.cache, Encoding.UTF8);
            this.csv = new CsvWriter(this.writer, this.config);
        }
    
        private void CreatePathAndFile(string filename)
        {
            // Generate path to file
            this.pathToFile = FilePathGenerator.AppendTimeStamp(filename);
            this.pathToFile = FilePathGenerator.AppendDataLogPath(this.pathToFile);

            // Create the file
            using FileStream fs = FileGenerator.CreateFileWithPermissions(this.pathToFile);
        }

        private (ILogEntity, ClassMap) GetClassAndMap(LogEntity entity)
        {
            switch(entity)
            {
                case LogEntity.Position:
                    return (new Position(), new PositionMap());
                case LogEntity.Time:
                    return (new Time(), new TimeMap());
                case LogEntity.Speed:
                    return (new Speed(), new SpeedMap());
                case LogEntity.Extrusion:
                    return (new Extrusion(), new ExtrusionMap());
                case LogEntity.Babystep:
                    return (new Babystep(), new BabystepMap());
                case LogEntity.Voltage:
                    return (new Voltage(), new VoltageMap());
                default:
                    throw new Exception(string.Format("LogEntity not recognized: {0}", entity.ToString()));
            }
        }
    }
}