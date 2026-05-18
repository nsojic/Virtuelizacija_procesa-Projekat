using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMetadata
    {
        [DataMember]
        public string T { get; set; }

        [DataMember]
        public string Pressure { get; set; }

        [DataMember]
        public string Tpot { get; set; }

        [DataMember]
        public string Tdew { get; set; }

        [DataMember]
        public string Rh { get; set; }

        [DataMember]
        public string Sh { get; set; }

        [DataMember]
        public string Date { get; set; }
    }
}