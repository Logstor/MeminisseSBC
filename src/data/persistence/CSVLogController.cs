using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

using CsvHelper;
using CsvHelper.Configuration;

namespace Meminisse
{
    public class CSVLogController<Entity, EntityMap> : ILogController<Entity>, IDisposable where EntityMap : ClassMap<Entity>
    {
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
            this.Clean();
        }

        void ILogController<Entity>.Init(string filename)
        {
            // Write header
            this.filename = filename;
            this.csv.WriteField("totTime"); // Manuel header element
            this.csv.WriteHeader<Entity>();
            this.csv.NextRecord();

            // Create file
            using FileStream fs = File.Create(filename);

            // Set initialized
            this.initialized = true;
        }

        void ILogController<Entity>.Reset()
        {
            // Clear and Create new instances
            this.filename = null;
            this.Clean();
            this.CreateStreams();

            // Set initialized
            this.initialized = false;
        }

        void ILogController<Entity>.Add(long elapsedTimeMs, Entity entity)
        {
            if (!this.initialized)
                throw new Exception("LogController not initialized!");

            // Write to cache
            this.csv.WriteField(string.Format("{0}", elapsedTimeMs));
            this.csv.WriteRecord<Entity>(entity);
            this.csv.NextRecord();
            this.csv.Flush();
        }

        void ILogController<Entity>.FlushToFile()
        {
            // Make sure things is initialized
            if (!this.initialized)
                throw new Exception("LogController not initialized!");

            try 
            {
                // Write to file
                using (FileStream fs = File.Open(filename, FileMode.Append))
                {
                    fs.Write(this.cache.GetBuffer(), 0, (int) this.cache.Length);
                }

                // Reset cache
                this.ResetCache();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
            this.csv = new CsvWriter(this.writer, CultureInfo.InvariantCulture);
            this.csv.Context.RegisterClassMap<EntityMap>();
        }
    }
}