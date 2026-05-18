using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ServiceResponse
    {
        [DataMember]
        public string Ack { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
