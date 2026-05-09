using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(WeatherService));

            try
            {
                host.Open();
                Console.WriteLine("WCF Service started...");
                Console.WriteLine("Press ENTER to stop service.");

                Console.ReadLine();

                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                host.Abort();
            }
        }
    }
}
