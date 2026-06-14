using Common;
using System;
using System.ServiceModel;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WeatherService : IWeatherService
    {
        private bool sessionStarted = false;
        private bool sessionCompleted = false;

        private readonly double rhThreshold;
        private readonly double tThreshold;
        private readonly double dewThreshold;
        private readonly double averageDeviationPercentage;

        private string sessionFolder;
        private string measurementsFilePath;
        private string rejectsFilePath;
        private SessionFileWriter sessionWriter;

        private int receivedSamples = 0;

        private readonly WeatherEventManager eventManager;

        public WeatherService()
        {
            eventManager = new WeatherEventManager();

            eventManager.OnTransferStarted += (s, e) => Console.WriteLine($"EVENT: {e.Message}");

            eventManager.OnSampleReceived += (s, e) => Console.WriteLine($"EVENT: {e.Message}");

            eventManager.OnTransferCompleted += (s, e) => Console.WriteLine($"EVENT: {e.Message}");

            eventManager.OnWarningRaised += (s, e) => Console.WriteLine($"WARNING: {e.Message}");
           
            rhThreshold = double.Parse(ConfigurationManager.AppSettings["RH_threshold"], CultureInfo.InvariantCulture);

            tThreshold = double.Parse(ConfigurationManager.AppSettings["T_threshold"], CultureInfo.InvariantCulture);

            dewThreshold = double.Parse(ConfigurationManager.AppSettings["DEW_threshold"], CultureInfo.InvariantCulture);

            averageDeviationPercentage = double.Parse(ConfigurationManager.AppSettings["AverageDeviationPercentage"], CultureInfo.InvariantCulture);
        }
        public ServiceResponse EndSession()
        {
            if (!sessionStarted)
            {
                return new ServiceResponse
                {
                    Ack = "NACK",
                    Status = "FAILED",
                    Message = "Session has not been started"
                };
            }

            if (sessionCompleted)
            {
                return new ServiceResponse
                {
                    Ack = "NACK",
                    Status = "FAILED",
                    Message = "Session has already been completed"
                };
            }

            sessionWriter?.Dispose();
            sessionWriter = null;

            Console.WriteLine($"Transfer completed. Samples received: {receivedSamples}");
            eventManager.RaiseTransferCompleted($"Transfer completed. Samples received: {receivedSamples}");
            
            sessionCompleted = true;
            sessionStarted = false;

            return new ServiceResponse
            {
                Ack = "ACK",
                Status = "COMPLETED",
                Message = "Session completed successfully"
            };
        }

        public ServiceResponse PushSample(WeatherSample sample)
        {
            if (!sessionStarted)
            {
                return new ServiceResponse
                {
                    Ack = "NACK",
                    Status = "FAILED",
                    Message = "Session has not been started"
                };
            }

            if (sessionCompleted)
            {
                return new ServiceResponse
                {
                    Ack = "NACK",
                    Status = "FAILED",
                    Message = "Session has already been completed"
                };
            }

            try
            {
                ValidateSample(sample);
            }
            catch (Exception ex)
            {

                string message = ex.Message;

                if (ex is FaultException<ValidationFault> vf)
                {
                    message = vf.Detail.Message;
                }

                sessionWriter.WriteReject(
                    $"{sample.Date}," +
                    $"{sample.T}," +
                    $"{sample.Pressure}," +
                    $"{sample.Tpot}," +
                    $"{sample.Tdew}," +
                    $"{sample.Rh}," +
                    $"{sample.Sh}," +
                    $"{message}");

                throw;
            }

            sessionWriter.WriteMeasurement(
                 $"{sample.Date}," +
                 $"{sample.T}," +
                 $"{sample.Pressure}," +
                 $"{sample.Tpot}," +
                 $"{sample.Tdew}," +
                 $"{sample.Rh}," +
                 $"{sample.Sh}");

            Console.WriteLine($"Transfer in progress... " + $"Sample received: {sample.Date}");
            eventManager.RaiseSampleReceived($"Sample received: {sample.Date}");
            receivedSamples++;

            return new ServiceResponse
            {
                Ack = "ACK",
                Status = "IN_PROGRESS",
                Message = "Sample received successfully"
            };
        }

        public ServiceResponse StartSession(SessionMetadata meta)
        {
            if (meta == null)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Session metadata cannot be null"
                    });
            }

            if (sessionStarted && !sessionCompleted)
            {
                return new ServiceResponse
                {
                    Ack = "NACK",
                    Status = "FAILED",
                    Message = "Session is already active"
                };
            }

            if (string.IsNullOrWhiteSpace(meta.T) ||
                string.IsNullOrWhiteSpace(meta.Pressure) ||
                string.IsNullOrWhiteSpace(meta.Tpot) ||
                string.IsNullOrWhiteSpace(meta.Tdew) ||
                string.IsNullOrWhiteSpace(meta.Rh) ||
                string.IsNullOrWhiteSpace(meta.Sh) ||
                string.IsNullOrWhiteSpace(meta.Date))
            {
                return new ServiceResponse
                {
                    Ack = "NACK",
                    Status = "FAILED",
                    Message = "All metadata fields are required"
                };
            }

            sessionFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Sessions");

            if (!Directory.Exists(sessionFolder))
            {
                Directory.CreateDirectory(sessionFolder);
            }

            measurementsFilePath = Path.Combine(sessionFolder, "measurements_session.csv");

            rejectsFilePath = Path.Combine(sessionFolder, "rejects.csv");

            sessionWriter?.Dispose();
            sessionWriter = new SessionFileWriter(measurementsFilePath, rejectsFilePath);

            Console.WriteLine("Transfer started");
            Console.WriteLine("Transfer in progress...");

            eventManager.RaiseTransferStarted("Transfer started successfully");

            Console.WriteLine($"T threshold: {tThreshold}");
            Console.WriteLine($"RH threshold: {rhThreshold}");
            Console.WriteLine($"DEW threshold: {dewThreshold}");
            Console.WriteLine($"Average deviation percentage: {averageDeviationPercentage}%");

            receivedSamples = 0;
            sessionStarted = true;
            sessionCompleted = false;

            return new ServiceResponse
            {
                Ack = "ACK",
                Status = "IN_PROGRESS",
                Message = "Session started successfully"
            };
        }

        private void ValidateSample(WeatherSample sample)
        {
            if (sample == null)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Sample cannot be null"
                    });
            }

            // Temperatura
            if (sample.T < -10 || sample.T > 35)
            {
                eventManager.RaiseWarning( $"Temperature out of range: {sample.T}");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Temperature must be between -10 and 35 degrees Celsius"
                    });
            }

            // Pritisak
            if (sample.Pressure < 900 || sample.Pressure > 1100)
            {
                eventManager.RaiseWarning($"Pressure out of range: {sample.Pressure}");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Pressure must be between 900 and 1100 hPa"
                    });
            }

            // Relativna vlažnost
            if (sample.Rh < 75 || sample.Rh > 100)
            {
                eventManager.RaiseWarning($"Relative humidity out of range: {sample.Rh}");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Relative humidity must be between 75 and 100%"
                    });
            }

            // Temperatura rosišta
            if (sample.Tdew < -10 || sample.Tdew > 10)
            {
                eventManager.RaiseWarning($"Dew point out of range: {sample.Tdew}");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Dew point must be between -10 and 10 degrees Celsius"
                    });
            }

            // Potencijalna temperatura
            if (sample.Tpot < 250 || sample.Tpot > 350)
            {
                eventManager.RaiseWarning($"Potential temperature out of range: {sample.Tpot}");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Potential temperature must be between 250 and 350 K"
                    });
            }

            // Specifična vlažnost
            if (sample.Sh < 0 || sample.Sh > 30)
            {
                eventManager.RaiseWarning($"Specific humidity out of range: {sample.Sh}");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Specific humidity must be between 0 and 30 g/kg"
                    });
            }

            // Datum i vrijeme
            if (sample.Date == default(DateTime))
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Date must be a valid date and time"
                    });
            }

            if (double.IsNaN(sample.T) || double.IsNaN(sample.Pressure) || double.IsNaN(sample.Rh))
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = "Invalid numeric format detected."
                    });
            }

        }
    }
}
