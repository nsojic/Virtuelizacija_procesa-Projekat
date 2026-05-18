using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Globalization;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IWeatherService> factory =
            new ChannelFactory<IWeatherService>("WeatherService");

            IWeatherService service = factory.CreateChannel();

            string csvPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "weather.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine("CSV file was not found.");
                return;
            }

            string rejectLogPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "rejects.log");

            if (File.Exists(rejectLogPath))
            {
                File.Delete(rejectLogPath);
            }

            List<WeatherSample> samples = new List<WeatherSample>();

            using (StreamReader reader = new StreamReader(csvPath))
            {
                string headerLine = reader.ReadLine();

                string line;

                int rowCount = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');

                    if (values.Length < 10)
                    {
                        File.AppendAllText(
                            rejectLogPath,
                            $"Invalid column count: {line}{Environment.NewLine}");

                        continue;
                    }

                    try
                    {
                        WeatherSample sample = new WeatherSample
                        {
                            Date = DateTime.ParseExact(
                            values[0],
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture),

                            Pressure = double.Parse(
                            values[1],
                            CultureInfo.InvariantCulture),

                            T = double.Parse(
                            values[2],
                            CultureInfo.InvariantCulture),

                            Tpot = double.Parse(
                            values[3],
                            CultureInfo.InvariantCulture),

                            Tdew = double.Parse(
                            values[4],
                            CultureInfo.InvariantCulture),

                            Rh = double.Parse(
                            values[5],
                            CultureInfo.InvariantCulture),

                            Sh = double.Parse(
                            values[9],
                            CultureInfo.InvariantCulture)
                        };

                        samples.Add(sample);

                        rowCount++;

                        if (rowCount >= 100)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(
                            rejectLogPath,
                            $"Parsing error: {line} | Error: {ex.Message}{Environment.NewLine}");
                    }
                }
            }

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
