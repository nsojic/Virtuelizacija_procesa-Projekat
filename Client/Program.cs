using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;

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
                Headers = new List<string>
                {
                    "T",
                    "Pressure",
                    "Tpot",
                    "Tdew",
                    "Rh",
                    "Sh",
                    "Date"
                }
            };

            string startResponse = service.StartSession(metadata);

            Console.WriteLine(startResponse);

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
                string response = service.PushSample(samples[i]);

                Console.WriteLine(response);
            }

            string endResponse = service.EndSession();

            Console.WriteLine(endResponse);
        }
    }
}
