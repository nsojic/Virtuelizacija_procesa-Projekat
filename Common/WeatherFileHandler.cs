using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WeatherFileHandler : IDisposable
    {
        private readonly string csvPath;

        private StreamReader reader;
        private StreamWriter rejectWriter;

        public WeatherFileHandler(string path)
        { 
            csvPath = path;
        }

        public StreamReader OpenReader()
        {
            try
            {
                reader = new StreamReader(csvPath);

                return reader;
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Error opening CSV file: {ex.Message}");

                return null;
            }
        }

        
           public void WriteReject(string errorType, string row, string message)
        {
            try
            {
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Logs",
                    "rejects.csv");

                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{errorType},{row},{message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reject log error: {ex.Message}");
            }
        }
        

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                try
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                        reader = null;
                    }

                    if (rejectWriter != null)
                    {
                        rejectWriter.Close();
                        rejectWriter.Dispose();
                        rejectWriter = null;
                    }

                    Console.WriteLine("Resources successfully released.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Dispose error: {ex.Message}");
                }
            }

            disposed = true;
        }

        ~WeatherFileHandler()
        {
            Dispose(false);
        }
    }
}
