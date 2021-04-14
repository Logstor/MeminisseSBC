using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

using CsvHelper;

namespace Meminisse
{
    public class CSVLogController<Entity> : ILogController<Entity>, IDisposable
    {
        private Mutex mut = new Mutex();
        private bool initialized = false;

        private MemoryStream cache;

        private StreamWriter writer;

        private CsvWriter csv;

        private string filename;

        public CSVLogController()
        {
            this.CreateStreams();
        }

        void IDisposable.Dispose()
        {
            this.csv.Dispose();
            this.writer.Dispose();
            this.cache.Dispose();
        }

        void ILogController<Entity>.Init(string filename)
        {
            // Write header
            this.filename = filename;
            this.csv.WriteHeader<Entity>();
            this.csv.NextRecord();

            // Create file
            using FileStream fs = File.Create(filename);

            // Set initialized
            this.initialized = true;
        }

        void ILogController<Entity>.Close()
        {
            // Clear and Create new instances
            this.Clean();
            this.CreateStreams();

            // Set initialized
        }

        void ILogController<Entity>.Add(Entity entity)
        {
            if (!this.initialized)
                throw new Exception("LogController not initialized!");

            // Write to cache
            this.csv.WriteRecord<Entity>(entity);
            this.csv.NextRecord();
            this.csv.Flush();
        }

        void ILogController<Entity>.FlushToFile()
        {
            // Make sure things is initialized
            if (!this.initialized)
                throw new Exception("LogController not initialized!");

            // Lock Mutex
            mut.WaitOne();

            try 
            {
                // Write to harddrive
                using (FileStream fs = File.Open(filename, FileMode.Append))
                {
                    fs.Write(this.cache.GetBuffer(), 0, (int) this.cache.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                // Unlock Mutex
                mut.ReleaseMutex();
            }
        }

        private void Clean()
        {
            this.csv.Dispose();
            this.writer.Dispose();
            this.cache.Dispose();
        }

        private void CreateStreams()
        {
            this.cache = new MemoryStream(128);
            this.writer = new StreamWriter(this.cache);
            this.csv = new CsvWriter(this.writer, CultureInfo.InvariantCulture);
        }
    }
}