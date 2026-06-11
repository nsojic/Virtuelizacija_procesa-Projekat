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

        public WeatherService()
        {
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

            Console.WriteLine("Session ended");

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
                using (StreamWriter writer =
                    new StreamWriter(rejectsFilePath, true))
                {
                    writer.WriteLine(
                        $"{sample?.Date}," +
                        $"{sample?.T}," +
                        $"{sample?.Pressure}," +
                        $"{sample?.Tpot}," +
                        $"{sample?.Tdew}," +
                        $"{sample?.Rh}," +
                        $"{sample?.Sh}," +
                        $"{ex.Message}");
                }

                throw;
            }

            using (StreamWriter writer =
            new StreamWriter(measurementsFilePath, true))
            {
                writer.WriteLine(
                    $"{sample.Date}," +
                    $"{sample.T}," +
                    $"{sample.Pressure}," +
                    $"{sample.Tpot}," +
                    $"{sample.Tdew}," +
                    $"{sample.Rh}," +
                    $"{sample.Sh}");
            }

            Console.WriteLine($"Received sample -> " +
                     $"Date={sample.Date}, " +
                     $"T={sample.T}, " +
                     $"Pressure={sample.Pressure}, " +
                     $"Tpot={sample.Tpot}, " +
                     $"Tdew={sample.Tdew}, " +
                     $"Rh={sample.Rh}, " +
                     $"Sh={sample.Sh}");

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

            sessionFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Sessions");

            if (!Directory.Exists(sessionFolder))
            {
                Directory.CreateDirectory(sessionFolder);
            }

            measurementsFilePath =
                Path.Combine(sessionFolder, "measurements_session.csv");

            rejectsFilePath =
                Path.Combine(sessionFolder, "rejects.csv");

            using (StreamWriter writer =
            new StreamWriter(measurementsFilePath, false))
            {
                writer.WriteLine(
                    "Date,T,Pressure,Tpot,Tdew,Rh,Sh");
            }

            using (StreamWriter writer =
            new StreamWriter(rejectsFilePath, false))
            {
                writer.WriteLine(
                    "Date,T,Pressure,Tpot,Tdew,Rh,Sh,Reason");
            }

            Console.WriteLine("Session started");

            Console.WriteLine($"T threshold: {tThreshold}");
            Console.WriteLine($"RH threshold: {rhThreshold}");
            Console.WriteLine($"DEW threshold: {dewThreshold}");
            Console.WriteLine($"Average deviation percentage: {averageDeviationPercentage}%");

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
            if (sample.T < -80 || sample.T > 60)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Temperature must be between -80 and 60 degrees Celsius"
                    });
            }

            // Pritisak
            if (sample.Pressure < 800 || sample.Pressure > 1200)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Pressure must be between 800 and 1200 hPa"
                    });
            }

            // Relativna vlažnost
            if (sample.Rh <= 0 || sample.Rh > 100)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Relative humidity must be between 0 and 100%"
                    });
            }

            // Temperatura rosišta
            if (sample.Tdew < -80 || sample.Tdew > 60)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Dew point must be between -80 and 60 degrees Celsius"
                    });
            }

            // Potencijalna temperatura
            if (sample.Tpot < -80 || sample.Tpot > 400)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Potential temperature must be between -80 and 400 degrees Celsius"
                    });
            }

            // Specifična vlažnost
            if (sample.Sh < 0 || sample.Sh > 50)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Specific humidity must be between 0 and 50 g/kg"
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
