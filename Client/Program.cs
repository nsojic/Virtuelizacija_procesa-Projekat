using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IWeatherService> factory =
            new ChannelFactory<IWeatherService>("WeatherService");

            IWeatherService service = factory.CreateChannel();

            var metadata = new SessionMetadata
            {
                T = "T",
                Pressure = "Pressure",
                Tpot = "Tpot",
                Tdew = "Tdew",
                Rh = "Rh",
                Sh = "Sh",
                Date = "Date"
            };

            ServiceResponse startResponse = service.StartSession(metadata);

            Console.WriteLine($"{startResponse.Ack} - {startResponse.Status} - {startResponse.Message}");

            var samples = new List<WeatherSample>
            {
                new WeatherSample
                {
                    T = 20,
                    Pressure = 1013,
                    Tpot = 19,
                    Tdew = 10,
                    Rh = 50,
                    Sh = 5,
                    Date = DateTime.Now
                },

                new WeatherSample
                {
                    T = 21,
                    Pressure = 1012,
                    Tpot = 20,
                    Tdew = 11,
                    Rh = 52,
                    Sh = 6,
                    Date = DateTime.Now
                }
            };

            for (int i = 0; i < samples.Count; i++)
            {
                ServiceResponse response = service.PushSample(samples[i]);

                Console.WriteLine($"{response.Ack} - {response.Status} - {response.Message}");
            }

            ServiceResponse endResponse = service.EndSession();

            Console.WriteLine($"{endResponse.Ack} - {endResponse.Status} - {endResponse.Message}");
        }
    }
}
