using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class WeatherSample
    {
        [DataMember]
        // temperatura
        public double T { get; set; }

        [DataMember]
        // pritisak     
        public double Pressure { get; set; }

        [DataMember]
        // potencijalna temperatura
        public double Tpot { get; set; }

        [DataMember]
        // temperatura rosišta
        public double Tdew { get; set; }

        [DataMember]
        // relativna vlažnost
        public double Rh { get; set; }

        [DataMember]
        // specifična vlažnost
        public double Sh { get; set; }

        [DataMember]
        // datum i vrijeme mjerenja
        public DateTime Date { get; set; }
    }
}
