using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SessionFileWriter : IDisposable
    {

        private StreamWriter measurementsWriter;
        private StreamWriter rejectsWriter;
        private bool disposed = false;

        public SessionFileWriter(string measurementsFilePath, string rejectsFilePath)
        {
            measurementsWriter = new StreamWriter(measurementsFilePath, false);
            measurementsWriter.WriteLine("Date,T,Pressure,Tpot,Tdew,Rh,Sh");
            measurementsWriter.Flush();

            rejectsWriter = new StreamWriter(rejectsFilePath, false);
            rejectsWriter.WriteLine("Date,T,Pressure,Tpot,Tdew,Rh,Sh,Reason");
            rejectsWriter.Flush();
        }

        public void WriteMeasurement(string line)
        {
            measurementsWriter.WriteLine(line);
            measurementsWriter.Flush();
        }

        public void WriteReject(string line)
        {
            rejectsWriter.WriteLine(line);
            rejectsWriter.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                measurementsWriter?.Close();
                measurementsWriter?.Dispose();
                measurementsWriter = null;

                rejectsWriter?.Close();
                rejectsWriter?.Dispose();
                rejectsWriter = null;

                Console.WriteLine("Session file resources released.");
            }

            disposed = true;
        }

        ~SessionFileWriter()
        {
            Dispose(false);
        }
    }
}
