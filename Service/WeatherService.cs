using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Console.WriteLine($"Received sample T={sample.T}");

            return "IN_PROGRESS";
        }

        public string StartSession(string meta)
        {
            Console.WriteLine("Session started");

            return "ACK";
        }
    }
}
