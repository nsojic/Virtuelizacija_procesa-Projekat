using Common;
using System;
using System.ServiceModel;


namespace Service
{
    public class WeatherService : IWeatherService
    {
        public string EndSession()
        {
            Console.WriteLine("Session ended");

            return "COMPLETED";
        }

        public string PushSample(WeatherSample sample)
        {
            ValidateSample(sample);

            Console.WriteLine($"Received sample T={sample.T}");

            return "IN_PROGRESS";
        }

        public string StartSession(string meta)
        {
            Console.WriteLine("Session started");

            return "ACK";
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
            if (sample.Rh < 0 || sample.Rh > 100)
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
