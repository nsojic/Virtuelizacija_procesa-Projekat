using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WeatherEventArgs : EventArgs
    {
        public string Message { get; set; }

        public WeatherEventArgs(string message)
        {
            Message = message;
        }
    }
}
