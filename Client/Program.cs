using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IWeatherService> factory = new ChannelFactory<IWeatherService>("WeatherService");

            IWeatherService service = factory.CreateChannel();

            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data","weather.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine("CSV file was not found.");
                return;
            }

            string logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Logs");

            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            string rejectLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "rejects.csv");

            if (File.Exists(rejectLogPath))
            {
                File.Delete(rejectLogPath);
            }

            List<WeatherSample> samples = new List<WeatherSample>();

            using (WeatherFileHandler handler = new WeatherFileHandler(csvPath))
            {
                StreamReader reader = handler.OpenReader();

                string headerLine = reader.ReadLine();

                string[] headers = headerLine.Split(',');

                Dictionary<string, int> columnIndexes = new Dictionary<string, int>();

                for (int i = 0; i < headers.Length; i++)
                {
                    columnIndexes[headers[i].Trim()] = i;
                }

                string[] requiredColumns = {
                                            "date",
                                            "p",
                                            "T",
                                            "Tpot",
                                            "Tdew",
                                            "rh",
                                            "sh"
                                        };

                foreach (string column in requiredColumns)
                {
                    if (!columnIndexes.ContainsKey(column))
                    {
                        Console.WriteLine($"Missing required column: {column}");

                        return;
                    }
                }

                string line;

                int rowCount = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');

                    try
                    {
                        WeatherSample sample = new WeatherSample
                        {
                            Date = DateTime.ParseExact(
                            values[columnIndexes["date"]],
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture),

                            Pressure = double.Parse(
                            values[columnIndexes["p"]],
                            CultureInfo.InvariantCulture),

                            T = double.Parse(
                            values[columnIndexes["T"]],
                            CultureInfo.InvariantCulture),

                            Tpot = double.Parse(
                            values[columnIndexes["Tpot"]],
                            CultureInfo.InvariantCulture),

                            Tdew = double.Parse(
                            values[columnIndexes["Tdew"]],
                            CultureInfo.InvariantCulture),

                            Rh = double.Parse(
                            values[columnIndexes["rh"]],
                            CultureInfo.InvariantCulture),

                            Sh = double.Parse(
                            values[columnIndexes["sh"]],
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
                        handler.WriteReject("Parsing error",line, ex.Message);
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
                if (i == 5)
                {
                    Console.WriteLine(
                        "Simulated connection interruption.");

                    break;
                }

                try
                {
                    ServiceResponse response =
                        service.PushSample(samples[i]);

                    Console.WriteLine($"{response.Ack} - " + $"{response.Status} - " + $"{response.Message}");
                }
                catch (FaultException<ValidationFault> ex)
                {
                    Console.WriteLine($"Validation error: {ex.Detail.Message}");

                    using (WeatherFileHandler handler = new WeatherFileHandler(""))
                    {
                        handler.WriteReject(
                            "Validation error",
                            samples[i].ToString(),
                            ex.Detail.Message);
                    }
                }
                catch (FaultException<DataFormatFault> ex)
                {
                    Console.WriteLine($"Format error: {ex.Detail.Message}");

                    using (WeatherFileHandler handler = new WeatherFileHandler(""))
                    {
                        handler.WriteReject(
                            "Format error",
                            samples[i].ToString(),
                            ex.Detail.Message);
                    }
                }
            }

            ServiceResponse endResponse = service.EndSession();

            Console.WriteLine($"{endResponse.Ack} - {endResponse.Status} - {endResponse.Message}");
        }
    }
}
